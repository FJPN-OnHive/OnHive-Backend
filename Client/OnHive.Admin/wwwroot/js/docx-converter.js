window.docxConverter = {
    convertToHtml: async function (fileBytes) {
        if (!window.mammoth) {
            throw new Error('Mammoth.js is not loaded.');
        }

        const mammothLib = window.mammoth;
        const arrayBuffer = new Uint8Array(fileBytes).buffer;

        const result = await mammothLib.convertToHtml(
            { arrayBuffer: arrayBuffer },
            {
                convertImage: mammothLib.images.inline(function (element) {
                    return element.read("base64").then(function (imageBuffer) {
                        return {
                            src: "data:" + element.contentType + ";base64," + imageBuffer
                        };
                    });
                })
            }
        );

        const cleaned = window.htmlCleaner.clean(result.value);
        return cleaned;
    }
};
