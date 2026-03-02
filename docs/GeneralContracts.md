### Contratos de API's

1. **Resposta padrão**

```json
{
  "code": 0,
  "message": "string",
  "payload": {}
}
```
* **Codes**
  * 0 - Retorno Ok
  * 1 - Retorno vazio
  * 2 - Alerta
  * 3 - Erro

1. **Resposta Paginada**

```json
{
  "code": 0,
  "message": "string",
  "payload": {
    "pageCount": 0,
    "page": 0,
    "itens": []
  }
}
```

3. **Filtro de consulta (Query)**
   
   ```url
   ?page=1&page_limit=100&filter=field1:eq:value1;field2:gte:value2;&sort=field1:asc;field2:desc
   ```

   * **page** 
     * Numero da página
     * Padrão = 0 (não paginar)
     * Opcional
   * **page_limit**
     * Quantidade de itens por página
     * Padrão = 0 (não paginar)
     * Opcional
   * **filter**
     * Filtro da consulta
     * Formato = campo:operador:valor
     * Múltiplos separados por ';'
       * Ex.: ``campo1:eq:valor;campo2:in:1,2,3``
     * Opcional
     * operadores válidos:
       * **eq** - igual
       * **gte** - maior ou igual
       * **gt** - maior
       * **lte** - menor ou igual
       * **lt** - menor
       * **ne** - diferente
       * **in** - contido em 
       * **nin** - não contido em
   * **sort**
     * Ordenação da consulta
     * Formato = campo:ordem
     * Múltiplos comandos separados por ';' 
       * Ex.: ``campo1:asc;campo2:desc``
     * Opcional
     * ordens válidas:
       * **asc** ou **a** - ascendente
       * **desc** ou **d** - descendente       

4. **Patch**

```json
{
    "id": "string",
    "tenantId": "string",
    "fields": {
        "campo1": "valor1",
        "campo2": 2,
        "campo3": false        
    }
}
```
    