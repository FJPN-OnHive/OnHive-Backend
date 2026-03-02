window.htmlCleaner = {
    clean: function (html) {
        if (!html) return '';

        const decodeHtml = (value) => {
            const textarea = document.createElement('textarea');
            textarea.innerHTML = value;
            return textarea.value;
        };

        const decoded = decodeHtml(typeof html === 'string' ? html : String(html));

        const parser = new DOMParser();
        const doc = parser.parseFromString(decoded, 'text/html');
        const root = doc.body;

        const allowedTags = new Set(['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'ul', 'ol', 'li', 'blockquote', 'br', 'hr', 'table', 'tr', 'td', 'th', 'thead', 'tbody', 'a', 'strong', 'em']);
        const removeTags = new Set(['script', 'style', 'meta', 'link', 'iframe', 'object', 'embed', 'head', 'title', 'img']);
        const unwrapTags = new Set(['span', 'div', 'font', 'o:p']);
        const allowedAttributes = {
            a: ['href']
        };

        const hasMarginLeft = (element) => {
            const style = element.getAttribute('style') || '';
            return /margin-left\s*:\s*[^0;]/i.test(style);
        };

        const removeWordConditionalComments = () => {
            let html = root.innerHTML;
            html = html.replace(/<!--\[if[^\]]*\]>[\s\S]*?<!\[endif\]-->/gi, '');
            html = html.replace(/<!--\[if[^\]]*\]-->/gi, '');
            html = html.replace(/<!--\[endif\]-->/gi, '');
            html = html.replace(/<!\[if[^\]]*\]>/gi, '');
            html = html.replace(/<!\[endif\]>/gi, '');
            root.innerHTML = html;
        };

        const getListType = (element) => {
            const text = element.textContent.trim();
            const style = element.getAttribute('style') || '';
            const bulletChars = ['●', '•', '·', '○', '■', '▪', '◦', '-'];

            if (!(/mso-list\s*:/i.test(style) ||
                (hasMarginLeft(element) && /text-indent\s*:\s*-/i.test(style)))) {
                return null;
            }

            if (/^(\d+[\.\)]\s*|[a-z][\.\)]\s*)/i.test(text)) {
                return 'ol';
            }

            if (bulletChars.some(b => text.startsWith(b))) {
                return 'ul';
            }

            return 'ul';
        };

        const cleanListItemText = (li) => {
            const walker = document.createTreeWalker(li, NodeFilter.SHOW_TEXT, null, false);
            const firstTextNode = walker.nextNode();
            if (firstTextNode) {
                let text = firstTextNode.textContent;
                text = text.replace(/^[\s●•·○■▪◦\-]+/, '');
                text = text.replace(/^\d+[\.\)]\s*/, '');
                text = text.replace(/^[a-z][\.\)]\s*/i, '');
                firstTextNode.textContent = text;
            }
        };

        const convertWordListsToHtml = () => {
            removeWordConditionalComments();

            const paragraphs = Array.from(root.querySelectorAll('p'));
            let i = 0;

            while (i < paragraphs.length) {
                const p = paragraphs[i];
                const listType = getListType(p);

                if (listType && p.parentNode) {
                    const list = doc.createElement(listType);
                    const parent = p.parentNode;
                    const nextSibling = p.nextSibling;

                    parent.insertBefore(list, p);

                    while (i < paragraphs.length) {
                        const currentP = paragraphs[i];
                        const currentType = getListType(currentP);

                        if (!currentType || currentType !== listType) break;
                        if (!currentP.parentNode) {
                            i++;
                            continue;
                        }

                        const li = doc.createElement('li');
                        while (currentP.firstChild) {
                            li.appendChild(currentP.firstChild);
                        }
                        cleanListItemText(li);
                        list.appendChild(li);
                        currentP.remove();
                        i++;
                    }
                } else {
                    i++;
                }
            }
        };

        convertWordListsToHtml();

        const normalizeText = (node) => {
            const cleanedText = node.textContent
                .replace(/\u00A0/g, ' ')
                .replace(/mso-[a-z\-]+:[^;>\n\r]+;?/gi, ' ')
                .replace(/style="[^"]*"/gi, ' ');
            node.textContent = cleanedText;
        };

        const renameElement = (element, newTag) => {
            const replacement = doc.createElement(newTag);
            while (element.firstChild) {
                replacement.appendChild(element.firstChild);
            }
            element.replaceWith(replacement);
            return replacement;
        };

        const unwrap = (element) => {
            const parent = element.parentNode;
            while (element.firstChild) {
                parent.insertBefore(element.firstChild, element);
            }
            element.remove();
        };

        const sanitizeAttributes = (element) => {
            const tag = element.tagName.toLowerCase();
            const whitelist = allowedAttributes[tag] || [];
            Array.from(element.attributes).forEach(attr => {
                const name = attr.name.toLowerCase();
                if (!whitelist.includes(name)) {
                    element.removeAttribute(attr.name);
                    return;
                }

                const value = (attr.value || '').trim();
                if (!value) {
                    element.removeAttribute(attr.name);
                    return;
                }

                if (name === 'href') {
                    const safe = value.startsWith('http://') || value.startsWith('https://') || value.startsWith('#') || value.startsWith('mailto:');
                    if (!safe) {
                        element.removeAttribute(attr.name);
                        return;
                    }
                    element.setAttribute(name, value);
                } else {
                    element.setAttribute(name, value);
                }
            });
        };

        const sanitize = (node) => {
            let child = node.firstChild;
            while (child) {
                const next = child.nextSibling;

                if (child.nodeType === Node.TEXT_NODE) {
                    normalizeText(child);
                    child = next;
                    continue;
                }

                if (child.nodeType !== Node.ELEMENT_NODE) {
                    child.remove();
                    child = next;
                    continue;
                }

                let tag = child.tagName.toLowerCase();
                if (tag === 'b') {
                    child = renameElement(child, 'strong');
                    tag = 'strong';
                } else if (tag === 'i') {
                    child = renameElement(child, 'em');
                    tag = 'em';
                }

                if (removeTags.has(tag)) {
                    child.remove();
                    child = next;
                    continue;
                }

                if (unwrapTags.has(tag) || !allowedTags.has(tag)) {
                    sanitize(child);
                    unwrap(child);
                    child = next;
                    continue;
                }

                sanitizeAttributes(child);
                sanitize(child);
                child = next;
            }
        };

        sanitize(root);

        const pruneNoiseText = (node) => {
            let child = node.firstChild;
            while (child) {
                const next = child.nextSibling;

                if (child.nodeType === Node.TEXT_NODE) {
                    const originalText = child.textContent;
                    const trimmed = originalText.trim();
                    if (!trimmed || /^[><]+$/.test(trimmed)) {
                        child.remove();
                        child = next;
                        continue;
                    }
                    child.textContent = originalText.replace(/[><]/g, ' ');
                } else if (child.nodeType === Node.ELEMENT_NODE) {
                    pruneNoiseText(child);
                }

                child = next;
            }
        };

        pruneNoiseText(root);

        root.querySelectorAll('a').forEach(el => {
            if (!el.textContent || !el.textContent.trim()) {
                if (!el.getAttribute('href')) {
                    el.remove();
                }
            }
        });

        root.querySelectorAll('strong, em').forEach(el => {
            if (!el.textContent || !el.textContent.trim()) {
                el.remove();
            }
        });

        const blockTags = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'li', 'blockquote'];
        blockTags.forEach(tag => {
            root.querySelectorAll(tag).forEach(el => {
                if (!el.textContent || !el.textContent.trim()) {
                    el.remove();
                }
            });
        });

        const prettyPrint = (node, level = 0) => {
            let result = '';
            const indent = '    '.repeat(level);
            
            const blockTags = new Set(['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'ul', 'ol', 'li', 'table', 'thead', 'tbody', 'tr', 'td', 'th', 'blockquote', 'div', 'hr']);
            const voidTags = new Set(['br', 'hr', 'img', 'input']);

            Array.from(node.childNodes).forEach(child => {
                if (child.nodeType === Node.TEXT_NODE) {
                    const text = child.textContent.replace(/\s+/g, ' ');
                    if (text.trim()) {
                        result += text;
                    }
                } else if (child.nodeType === Node.ELEMENT_NODE) {
                    const tagName = child.tagName.toLowerCase();
                    const isBlock = blockTags.has(tagName);
                    const isVoid = voidTags.has(tagName);

                    if (isBlock && result.trim().length > 0) {
                        result += '\n' + indent;
                    } else if (isBlock) {
                        result += indent;
                    }

                    let attrs = '';
                    Array.from(child.attributes).forEach(attr => {
                        attrs += ` ${attr.name}="${attr.value}"`;
                    });

                    result += `<${tagName}${attrs}`;

                    if (isVoid) {
                        result += ' />';
                    } else {
                        result += '>';
                        
                        const hasBlockChildren = Array.from(child.children).some(c => blockTags.has(c.tagName.toLowerCase()));
                        
                        if (hasBlockChildren) {
                            result += '\n' + prettyPrint(child, level + 1);
                            result += '\n' + indent + `</${tagName}>`;
                        } else {
                            const childContent = prettyPrint(child, level + 1); 
                            result += childContent;
                            result += `</${tagName}>`;
                        }
                    }
                    
                    if (isBlock) {
                        result += '\n';
                    }
                }
            });
            return result;
        };

        root.innerHTML = root.innerHTML
            .replace(/&nbsp;/gi, ' ')
            .replace(/([•●·])\s*/g, '');

        return prettyPrint(root).trim();
    }
};
