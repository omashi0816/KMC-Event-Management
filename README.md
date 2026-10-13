# KMC Event Management System

A full-stack Event Management System developed using **ASP.NET Core Web API** and an **ASP.NET Core MVC client**. The project is organized into separate backend, frontend, and testing projects.

## Project Overview

The KMC Event Management System is a software development project designed with a modular structure to separate API functionality, the client application, and automated testing.

## Project Structure

```text
KMCEventAPI/
├── KMCEventAPI/          # Backend Web API
├── KMCEventClient/       # Frontend MVC client
├── KMCEventAPI.Tests/    # Test project
├── KMCEventAPI.slnx      # Solution file
└── .gitignore
```

## Technologies Used

- C#
- ASP.NET Core Web API
- ASP.NET Core MVC
- .NET
- Swagger / OpenAPI
- Visual Studio
- Visual Studio Code
- Git and GitHub

## Application Architecture

The project separates its main responsibilities into three components:

- **Backend API:** Handles server-side application logic and API requests.
- **Frontend MVC Client:** Provides the web-based client interface.
- **Test Project:** Contains tests for the backend application.

## Getting Started

### Prerequisites

Install the following tools before running the project:

- .NET SDK compatible with the solution
- Visual Studio or Visual Studio Code
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/omashi0816/KMC-Event-Management.git
cd KMC-Event-Management
```

### 2. Restore Dependencies

Run this command from the project root:

```bash
dotnet restore KMCEventAPI.slnx
```

### 3. Configure the Application

Configure the required local application settings and database connection details according to the project configuration.

Do not commit passwords, API keys, connection-string credentials, or other sensitive information to GitHub.

### 4. Run the Backend API

```bash
dotnet run --project KMCEventAPI/KMCEventAPI.csproj
```

When running with the local configuration used during development, the Swagger interface has been accessed at:

`https://localhost:7285/swagger`

The actual URL may differ depending on the launch settings.

### 5. Run the Frontend Client

Open the solution in Visual Studio, locate the MVC client project, configure its required settings, and run it using the appropriate launch profile.

### 6. Run Tests

Open the solution in Visual Studio and use Test Explorer to discover and run the available tests.

## Repository

GitHub: https://github.com/omashi0816/KMC-Event-Management

## Developer

**Omashi Sellahewa**

Software Engineering | Web Application Development | C# | ASP.NET Core

