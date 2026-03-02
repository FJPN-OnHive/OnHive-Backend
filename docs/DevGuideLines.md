# Geral

#### Versão .Net

.net 10.0 +

#### Língua do código

* Todo código, comentários, documentação, Documentação OpenAPI (Swagger), devem ser escritos em inglês.

* Variáveis, métodos, classes, e entidades devem ser escritos em inglês.

* Tabelas, colunas, filas de mensagens, logs e tracings, devem ser escritos em inglês.

#### Padrões de codificação e boas praticas

Utilizaremos como base o [C# Coding Standards Best Practices](https://www.dofactory.com/csharp-coding-standards) do Dofactory, por ser um compilado de boas praticas de mercado, contendo padrões recomendados pela própria microsoft.
Também podemos consultar o [C# Code Convention da Microsoft](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions), em caso de duvidas.

1. **Nome de classes e métodos:**

   * PascalCasing 

   * Iniciando com letra maiúscula

   * As classes devem ser nomeadas com substantivos.

   * Os métodos devem ser nomeados com verbos ou frases verbais.

   * Não utilizar underline '_'.

    ```csharp
    public class ClientActivity
    {
        public void ClearStatistics()
        {
            //...
        }
        public void CalculateStatistics()
        {
            //...
        }
    }
    ```

2. **Nome de interfaces:**

   * As interfaces seguem as mesma regras de nomenclatura das classes, com a adição de um 'I' maiúsculo no inicio.

    ```csharp
    public interface IShape
    {
    }
    public interface IShapeCollection
    {
    }
    public interface IGroupable
    {
    }
    ```

3. **Nomes de variáveis e argumentos:**

   * CamelCasing

   * Iniciando com letra minúscula.

   * Não iniciar com numero ou símbolos 

   * Não iniciar com underline '_'

    ```csharp
    public class UserLog
    {
        public void Add(LogEvent logEvent)
        {
            int itemCount = logEvent.Items.Count;
            // ...
        }
    }
    ```

4. **Identificadores**

    * Não utilizar identificadores de tipo (Hungarian Notation)*

    ```csharp
    // Correto
    int counter;
    string name;

    // Evitar
    int iCounter;
    string strName;
    ```

5. **Constantes**

    * PascalCasing 

    * Iniciando com letra maiúscula

    * Não utilizar underline '_'

    * Não utilizar todas as letras maiúsculas.

    ```csharp
    // Correto
    public static const string ShippingType = "DropShip";

    // Evitar
    public static const string SHIPPINGTYPE = "DropShip";
    ```

6. **Abreviações**

    * Evitar utilizar abreviações

    * Com exceção das comumente utilizadas:

      * Id

      * Xml

      * Ftp

      * Uri

      * Api

      * Html

    * Ao utilizar abreviações utilize PascalCasing para abreviações com 3 ou mais caracteres, para as com 2 caracteres ambas as letras maiúsculas.

    ```csharp
    HtmlHelper htmlHelper;
    FtpTransfer ftpTransfer;
    UIControl uiControl;
    ```

7. **Nomes de Tipos**

   * Use nomes predefinidos (int, string, bool) ao invés de nomes de sistema (Int16, Single, Uint64).

   ```csharp
   // Correto
   string firstName;
   int lastIndex;
   bool isSaved;

   // Evitar
   String firstName;
   Int32 lastIndex;
   Boolean isSaved;
   ```

8. **Tipos implícitos**

   * Utilize tipos implícitos nas declarações de variáveis sempre que possível (var).

   * Com exceção de tipos primitivos e não inicializados.

   ```csharp
     var stream = File.Create(path);
     var customers = new Dictionary();
     
     // exceção
     int index = 100;
     string timeSheet;
     bool isCompleted;
   ```

9. **Nomes de arquivos**

   * O nome dos arquivos devem seguir os nomes da classe principal que contém.

    ```csharp
    // Localizado em Client.cs
    public class Client
    {
        //...
    }
    ```
10. **Namespaces**

    * Devem refletir a estrutura de pasta que os contém.

    * Devem ser organizados com um estrutura clara e definida.

    * Sempre que possível devem iniciar com o nome da companhia seguido do nome do projeto, caso contrario iniciar com o nome do projeto.

    ```csharp
    namespace Company.Product.Module.SubModule
    namespace Product.Module.Component
    namespace Product.Layer.Module.Group
    ```

11. **Chaves {}**

    * Devem ser alinhadas verticalmente.

    * Podem ser suprimidas em alguns casos:

      * Namespaces.
      * Usings.

    ```csharp
    namespace Company.Product.Module.SubModule;
    class Program
    {
        static void Main(string[] args)
        {
            using(var file = File.OpenRead(args[0]));
        }
    }
    ```

12. **Variáveis de classe**

    * Declare as variáveis e propriedades de classe no topo da classe.

    * As variáveis estáticas devem vir primeiro.

    ```csharp
    public class Account
    {
        public static string BankName;
        public static decimal Reserves;
    
        public string Number {get; set;}
        public DateTime DateOpened {get; set;}
        public DateTime DateClosed {get; set;}
        public decimal Balance {get; set;}
    
        // Constructor
        public Account()
        {
            // ...
        }
    }
    ```

13. **Enums**

    * Devem ser nomeados no singular.

    * Com exceção de enums 'bit field'.

    * Enums 'bit field' devem ser anotados com [Flags]

    * Não especificar o tipo do enum.

    * Não utilizar o sufixos nos enums, (como enum, type, etc.)

    ```csharp
    // Correto
    public enum Color
    {
        Red,
        Green,
        Blue,
        Yellow,
        Magenta,
        Cyan
    }

    // exceção (bit field)
    [Flags]
    public enum Dockings
    {
        None = 0,
        Top = 1, 
        Right = 2, 
        Bottom = 4,
        Left = 8
    }

    // Evitar (tipo especificado)
    public enum Direction : long
    {
        North = 1,
        East = 2,
        South = 3,
        West = 4
    }

    // Evitar (Sufixo)
    public enum CoinEnum
    {
        Penny,
        Nickel,
        Dime,
        Quarter,
        Dollar
    }
    ```

14. **Ordem de elementos dentro da classe**
    
    Os elementos de uma classe deve seguir uma ordem baseada em seus tipos (variável, propriedades, construtores, métodos, etc.), a ordem deve ser a seguinte (de acordo com StyleCopAnalyzers):

    * Constantes.

    * Campos (Variáveis de classe).

    * Construtores.

    * Finalizadores.

    * Delegates.

    * Eventos.

    * Enums.

    * Implementação de interfaces.

    * Propriedades.

    * Indices.

    * Métodos.

    * Structs.

    * Classes.

    Dentro destes grupos, os mesmo devem ser ordenados por acessibilidade.

    * public

    * internal

    * protected internal

    * protected

    * private

    Dentro destes grupos devem ser ordenados por tipo de instancia:

    * static

    * non-static

15. **Documentação de API**

    * Toda API deve ser documentada, utilizando o OpenAPI, Swagger.

    * Aplicações web em dotnet já vem com a configuração OpenAPI configurada.

    * O Swagger deve estar apenas disponível em ambientes locais, de desenvolvimento e de homologação, nunca em ambientes produtivos.

#### Template dos serviços

1. **Nome dos serviços:**

   * Devem iniciar com o nome do projeto, seguidos do nome da solução e do tipo da solução.

   * Tipos de solução:

     * Api

     * Library

     * Worker

     * Job

     * Tool

  Exemplo: Sendo uma  solução de uma library chamada 'Core' ficaria: OnHive.Core.Library

2. **Nome dos repositórios:**

    * Devem ser compostos pelo nome da solução substituindo o '.' por '_', exemplo: solução OnHive.Core.Library, ficaria OnHive_core_library.

3. **Estrutura geral da solução:**

    Cada solução e composta de um conjunto de projetos, separados por domínios:

    * Application 

      * Deve ter o nome da solução.

      * Contém a entrypoint da solução.

      * Contém Endpoints no caso das API's.

      * Contém Worker no caso dos workers.

      * Contém o Job no caso dos Jobs.

      * Referencia todos os outros projetos para configurar a injeção de dependência.

    * Domain

      * Deve ter o nome da solução seguido de .Domain

      * Deve conter todos os Models e Entidades da solução.

      * Deve conter as constantes da solução.

      * Deve conter as abstrações (Interfaces) da solução.

      * Esse projeto é referenciado por todos os outros.

    * Services

      * Deve ter o nome da solução seguido de .Services

      * Contém as classes de regras de negocio.

      * Referencia apenas o Domain, e implementa as interfaces contidas nele.

    * Repositories

      * Deve ter o nome da solução seguido de .Repositories

      * Contém as classe de aceso a base de dados, caso exista.

      * Referencia apenas o Domain, e implementa as interfaces contidas nele.

    * Tests

      * Deve ter o nome da solução seguido de .Tests

      * Contem os testes unitários da solução.

  Visão geral da estrutura de pasta de uma solução típica:

    * OnHive.Example.Api (Application)

      * DependencyInjection/

        * ServicesExtenssions.cs

      * HealthChecks/

        * HealthChecks.cs

      * Controllers/

        * ExampleController.cs

      * Workers/

        * ExampleWorker.cs

    * Program.cs

  * OnHive.Example.Domain

      * Abstractions/

        * Services/

          * IExampleService.cs

        * Repositories/

          * IExampleRepository.cs

      * Models/

        * Example.cs

      * Entities/

        * ExampleEntity.cs

   * OnHive.Example.Services

      * ExampleService.cs

   * OnHive.Example.Repositories

      * ExampleRepository.cs

   * OnHive.Example.Tests

      * Services/

        * ServiceTests.cs

#### Testes Unitários

1. **Framework de testes unitários**

   * [Xunit](https://www.nuget.org/packages/xunit)

2. **Bibliotecas Auxiliares**

   * [MOQ](https://www.nuget.org/packages/Moq)

   * [FluentAssertions](https://www.nuget.org/packages/FluentAssertions)

3. **Cobertura de testes**

 A cobertura de testes unitários deve ser no mínimo 85% para novos projetos.

4. **Exclusões da cobertura**

 Alguns elementos de código não são passiveis de testes unitários, sendo estes testados pelos testes integrados, são eles:

   * Inicialização (Main, Program, Startup).

   * Controllers (API, Endpoints).

   * Injeção de dependência.

   * Classes de acesso a repositórios (Banco de dados).

 Podem haver outras exclusões, dependendo de cada caso, no geral uma classe ou serviço que não possa ser simulado no contexto de testes unitários, deve ser excluído da cobertura, e ser devidamente testado no contexto de testes integrados.

#### Testes Integrados

1. **Conceito**

 Execução de testes integrados automatizados em um ambiente isolado, com todos os serviços em funcionamento e com o banco de dados contendo dados de teste.

2. **Framework**
1. 
1. 
   * [Cypress](https://www.cypress.io)

3. **Estrutura**

 Os testes integrados diferente dos testes unitários, são implementados em um projeto separado, que executa os testes de maneira autônoma, apontando para um ambiente isolado, que pode estar na nuvem, em um agente do pipeline ou montado localmente.



#### Extensões do Visual Studio 


1. **CodeMaid VS2022**

   * Para limpeza e organização automática de código.

2. **SonarLint for Visual Studio 2022**

   * Analise de código estático.

3. **SpecFlow for Visual Studio 2022**

   * Framework de testes integrados e BDD.

4. **Visual Studio Spell Checker (VS2022)**

   * Corretor ortográfico.

5. **Unit Test Boilerplate Generator**

   * Gerador de código base para testes unitários.