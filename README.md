<div align="center">
  <img src="wwwroot/images/PonteInclusaoLogo.svg" alt="Logo Ponte Inclusão" width="200"/>
  <h1>Ponte Inclusão</h1>
</div>

<p align="center">
  <strong>Conectando Pessoas com Deficiência a Oportunidades Educacionais Inclusivas.</strong>
  <br />
  <br />
  <img alt="Status do Projeto" src="https://img.shields.io/badge/status-MVP%20Funcional-green">
  <img alt="Linguagem" src="https://img.shields.io/badge/language-C%23%2012-blueviolet">
  <img alt="Framework" src="https://img.shields.io/badge/framework-.NET%208-blue">
</p>

---

## Sobre o Projeto

**Ponte Inclusão** é uma aplicação web desenvolvida para solucionar um desafio real: a dificuldade enfrentada por pais e responsáveis de Pessoas com Deficiência (PCDs) em encontrar instituições de ensino que ofereçam o suporte necessário para um desenvolvimento pleno e inclusivo.

A plataforma permite que usuários pesquisem por cidade e por tipo de necessidade específica, retornando uma lista de locais relevantes com informações básicas, e facilitando o primeiro contato.

Algumas funcionalidades podem estar ausentes por questões da interação do app com as API's externas.

### 🎯 Contexto Acadêmico

Este projeto foi desenvolvido como requisito para a **Atividade Extensionista II** do curso de **Análise e Desenvolvimento de Sistemas**. O trabalho está alinhado ao tema **"Ferramenta de Apoio à Inclusão e Acessibilidade"**, buscando aplicar os conceitos da análise de requisitos, arquitetura e desenvolvimento eficiente na criação de uma solução com impacto social positivo, que visa diminuir barreiras e promover a inclusão no ambiente educacional.

---

## Funcionalidades

* **Busca Inteligente:** Pesquisa de instituições por cidade e tipo de suporte necessário (ex: Autismo, Surdez, Acessibilidade para Cadeirantes).
* **Listagem de Resultados:** Exibição clara dos locais encontrados, com nome, endereço e avaliação.
* **Integração com Google Maps:** Cada resultado é um link direto para a localização no Google Maps.
* **Plano B Inteligente:** Caso nenhuma escola seja encontrada, o sistema sugere o contato do órgão de educação da cidade pesquisada como um caminho alternativo.

---

## Tecnologias Utilizadas

A aplicação foi construída com uma arquitetura moderna e robusta, focada em segurança e manutenibilidade.

* **Backend & Lógica de Negócio:**
    * C# 12 e .NET 8
    * ASP.NET Core
    * Princípios SOLID e Programação Orientada a Objetos

* **Frontend:**
    * Blazor Server (para uma interface reativa e 100% em C#)
    * HTML5 e CSS3 (para estrutura e estilo)

* **APIs Externas:**
    * Google Maps Places API (para busca de locais)

* **Segurança:**
    * .NET User Secrets (para armazenamento seguro da chave da API em desenvolvimento)
    * Pronto para integração com Google Secret Manager ou Azure Key Vault em produção.

* **Hospedagem (Deploy):**
    * A aplicação foi projetada para ser publicada como um contêiner Docker no **Azure App Service** ou **Google Cloud Run**.

---

## Como Executar o Projeto Localmente

Para rodar o projeto em sua máquina local, siga os passos abaixo.

### Pré-requisitos

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior.
* Git.
* Um editor de código como o [VS Code](https://code.visualstudio.com/).
* Uma chave da API do Google Cloud com a "Places API" ativada.

### Passos para Instalação

1.  **Clone o repositório:**
    ```bash
    git clone https://github.com/Lilja-J/PonteInclusaoWeb.git
    cd PonteInclusaoWeb
    ```

2.  **Configure a Chave da API (Passo Crucial de Segurança):**
    Este projeto usa o **.NET User Secrets** para proteger a chave da API e evitar que ela seja enviada para o GitHub.
    
    Abra um terminal na pasta do projeto e execute os seguintes comandos:
    
    * Primeiro, para inicializar o sistema de segredos no projeto:
        ```bash
        dotnet user-secrets init
        ```
    * Depois, para guardar sua chave de forma segura (substitua `SUA_CHAVE_AQUI` pela sua chave real):
        ```bash
        dotnet user-secrets set "GoogleMaps:ApiKey" "SUA_CHAVE_AQUI"
        ```

3.  **Restaure as dependências do projeto:**
    ```bash
    dotnet restore
    ```

4.  **Execute a aplicação:**
    ```bash
    dotnet run
    ```
    O terminal mostrará uma URL (ex: `http://localhost:5200`). Abra-a no seu navegador para ver o aplicativo funcionando.

### Configuração em Produção (Docker / Azure / GCP)

Para ambientes de produção, **nunca utilize arquivos de configuração com chaves comitadas**. A chave da API deve ser injetada via variável de ambiente da plataforma de hospedagem:

* **Nome da variável de ambiente:** `GoogleMaps__ApiKey` (formato padrão do .NET com duplo sublinhado para seções aninhadas)
* **Docker:**
  ```bash
  docker run -p 8080:8080 -e GoogleMaps__ApiKey="SUA_CHAVE_REAL" ponteinclusaoweb:latest
  ```
* **Azure App Service:** Em *Configuration > Application Settings*, adicione `GoogleMaps:ApiKey` ou `GoogleMaps__ApiKey`.

---

## Estrutura do Projeto

```
/PonteInclusaoWeb
│
├── Common/                   # Utilitários de segurança e validação (ex: SecurityValidator.cs)
│
├── Components/               # Componentes Blazor (páginas, layouts)
│   ├── Pages/                # Páginas roteáveis da aplicação (ex: Home.razor, Error.razor)
│   ├── Routes.razor         # O roteador principal do Blazor
│   └── App.razor            # O componente raiz do Blazor
│
├── Models/                  # Classes que representam os dados (ex: Place, Coordinates, etc.)
│
├── Services/                # Classes com a lógica de negócio (ex: GoogleMapsService)
│
├── wwwroot/                 # Arquivos estáticos (CSS, imagens, JavaScript)
│
├── _Imports.razor           # Diretivas @using globais para os componentes Blazor
│
├── Program.cs               # Ponto de entrada, middlewares de segurança, Rate Limiting e DI
│
└── PonteInclusaoWeb.csproj  # Arquivo de projeto .NET com UserSecrets configurado
```

---

##  Contato

**Jean Vitor Lilja** - [LinkedIn](https://www.linkedin.com/in/jean-lilja)
