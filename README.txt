================================================================================
  MICROSERVICES SOLUTION — E-Commerce Platform
  .NET 8 | Clean Architecture | CQRS | DDD | Docker | Kubernetes
================================================================================

DESCRIPCION GENERAL
-------------------
Plataforma de e-commerce simulada construida con arquitectura de microservicios.
Cuatro servicios independientes que se comunican de forma asincrona via RabbitMQ
y de forma sincrona via gRPC. Cada servicio tiene su propia base de datos
PostgreSQL (Database-per-Service pattern).

LENGUAJES PRINCIPALES
---------------------
  • C#                       Lógica de negocio, APIs, tests (lenguaje principal)
  • YAML                     Docker Compose, Kubernetes, GitHub Actions
  • JSON                     Configuración de aplicaciones
  • Protocol Buffers         Definiciones gRPC
  • XML                      Archivos de proyecto (.csproj), MSBuild
  • Dockerfile               Contenerización de servicios
  • Bash/Shell               Scripts y CI/CD

Desarrollado para demostrar conocimiento en:
  - Clean Architecture con 4 capas por servicio (Domain, Application, Infrastructure, API)
  - CQRS con MediatR (Commands y Queries separados)
  - Domain-Driven Design (Aggregates, Value Objects, Domain Events)
  - Seguridad JWT con RS256, BCrypt y refresh tokens rotativos
  - Mensajeria asincrona con MassTransit + RabbitMQ
  - Comunicacion sincrona entre servicios con gRPC + Protobuf
  - API Gateway con YARP (reverse proxy)
  - Observabilidad con OpenTelemetry + Jaeger
  - Contenerizacion con Docker y orquestacion con Kubernetes (AKS)
  - CI/CD con GitHub Actions


================================================================================
  SERVICIOS Y PUERTOS
================================================================================

  SERVICIO           PUERTO    SWAGGER                     DESCRIPCION
  -----------------------------------------------------------------------
  API Gateway        5000      http://localhost:5000        Punto de entrada unico
  Orders Service     5001      http://localhost:5001/swagger  Gestion de ordenes
  Products Service   5002      http://localhost:5002/swagger  Catalogo de productos
  Payments Service   5003      http://localhost:5003/swagger  Procesamiento de pagos
  Auth Service       5004      http://localhost:5004/swagger  Autenticacion y JWT

  INFRAESTRUCTURA
  -----------------------------------------------------------------------
  PostgreSQL (Orders)    5433   BD exclusiva del servicio de ordenes
  PostgreSQL (Products)  5434   BD exclusiva del servicio de productos
  PostgreSQL (Payments)  5435   BD exclusiva del servicio de pagos
  PostgreSQL (Auth)      5436   BD exclusiva del servicio de auth
  RabbitMQ               5672   Message broker (AMQP)
  RabbitMQ Dashboard     15672  http://localhost:15672 (guest/guest)
  Jaeger UI              16686  http://localhost:16686 (trazas distribuidas)


================================================================================
  ARQUITECTURA
================================================================================

ESTRUCTURA DE CAPAS (por servicio)
  Domain           — Entidades, Value Objects, Domain Events, Excepciones
  Application      — Commands, Queries, Handlers, DTOs, Validators, Interfaces
  Infrastructure   — EF Core, Repositories, MassTransit consumers, gRPC clients
  API              — Controllers, Middleware, Swagger, DI setup

COMUNICACION ENTRE SERVICIOS

  [Cliente]
      |
      v
  [API Gateway :5000]  — YARP reverse proxy, enruta por path
      |
      +---> /api/orders/*    --> Orders Service :5001
      +---> /api/products/*  --> Products Service :5002
      +---> /api/payments/*  --> Payments Service :5003
      +---> /api/auth/*      --> Auth Service :5004

  FLUJO COMPLETO DE UNA ORDEN:

  1. POST /api/orders                        (HTTP sincrono)
     Orders Service verifica stock           (gRPC -> Products Service)
     Orders Service crea la orden
     Orders Service publica OrderCreatedIntegrationEvent  (RabbitMQ)

  2. Payments Service consume OrderCreatedIntegrationEvent
     Simula procesamiento de pago
     Publica PaymentProcessedIntegrationEvent  (RabbitMQ)

  3. Orders Service consume PaymentProcessedIntegrationEvent
     Si pago exitoso  -> Order.Confirm()
     Si pago fallido  -> Order.Cancel(razon)


================================================================================
  AUTH SERVICE — SEGURIDAD
================================================================================

Implementacion de seguridad de nivel produccion:

  RS256 (RSA 2048-bit)
    - Firma asimetrica: clave privada solo en auth-service
    - Clave publica expuesta en /.well-known/jwks.json (estandar OpenID Connect)
    - Otros servicios pueden validar tokens sin compartir secretos

  BCrypt (work factor 12)
    - ~300ms por hash — resistente a ataques de fuerza bruta con GPU
    - Factor 12 es el estandar de industria para aplicaciones modernas

  Refresh Token Rotation
    - Access token: 15 minutos de duracion
    - Refresh token: 7 dias, rotado en cada uso
    - Token almacenado como SHA256(token) en BD — si la BD se filtra, inutilizable
    - Revocacion inmediata disponible (logout)

  Timing-safe Login
    - BCrypt se ejecuta siempre, aunque el usuario no exista
    - Previene user enumeration por diferencia de tiempo de respuesta

  Rate Limiting
    - 5 peticiones por minuto por IP en /login y /register
    - HTTP 429 si se supera el limite

  Password Policy (FluentValidation)
    - Minimo 8 caracteres, maximo 128
    - Requiere: mayuscula, minuscula, numero, caracter especial

  ENDPOINTS AUTH
    POST   /api/auth/register   Crear cuenta (rate limited)
    POST   /api/auth/login      Obtener tokens (rate limited)
    POST   /api/auth/refresh    Rotar refresh token
    POST   /api/auth/revoke     Logout — revocar token  [Authorize]
    GET    /api/auth/me         Perfil del usuario       [Authorize]
    GET    /.well-known/jwks.json  Clave publica RSA (cache 1h)

  ROLES
    Customer   — rol por defecto al registrarse
    Admin      — acceso a operaciones administrativas
    Politicas: AdminOnly, CustomerOrAdmin


================================================================================
  ENDPOINTS POR SERVICIO
================================================================================

ORDERS SERVICE  (http://localhost:5001/swagger)
  GET    /api/orders/{id}                    Obtener orden por ID
  GET    /api/orders/customer/{customerId}   Ordenes de un cliente
  POST   /api/orders                         Crear nueva orden
  DELETE /api/orders/{id}                    Cancelar orden

  Orden -> estados: Pending -> Confirmed / Cancelled
  Validacion de stock via gRPC al crear la orden

PRODUCTS SERVICE  (http://localhost:5002/swagger)
  GET    /api/products              Listar productos (filtro por categoria)
  GET    /api/products/{id}         Obtener producto por ID
  POST   /api/products              Crear producto
  PATCH  /api/products/{id}/stock   Ajustar stock (add / reduce)
  DELETE /api/products/{id}         Desactivar producto

  Expone tambien servidor gRPC en el mismo puerto:
    CheckStock(productId, quantity) -> bool
    GetProductDetails(productId)    -> nombre, precio, stock

PAYMENTS SERVICE  (http://localhost:5003/swagger)
  GET    /api/payments/order/{orderId}   Estado de pago de una orden
  POST   /api/payments                   Procesar pago manualmente

  Logica de simulacion:
    Monto < $10,000  ->  pago exitoso
    Monto >= $10,000 ->  pago rechazado ("Amount exceeds transaction limit")

AUTH SERVICE  (http://localhost:5004/swagger)
  Ver seccion AUTH SERVICE arriba


================================================================================
  LENGUAJES Y TECNOLOGÍAS
================================================================================

LENGUAJES DE PROGRAMACION
  • C# 12               — Lenguaje principal para toda la lógica de negocio
  • YAML                — Configuración de Docker Compose, Kubernetes, GitHub Actions
  • JSON                — Configuración de aplicaciones (appsettings.json)
  • XML                 — Project files (.csproj), MSBuild configuration
  • Protocol Buffers    — Definición de servicios gRPC (products.proto)
  • SQL                 — Queries EF Core, migraciones (implícito en Npgsql)
  • Bash/Shell          — Scripts de Docker, CI/CD pipelines
  • Dockerfile          — Multi-stage builds para cada servicio

FRAMEWORKS Y RUNTIMES
  • .NET 8              — Runtime y framework principal
  • ASP.NET Core 8      — Framework web para APIs REST
  • Entity Framework Core 8  — ORM para persistencia en PostgreSQL

PATRONES Y ARQUITECTURA
  • Clean Architecture  — Separación en 4 capas por servicio
  • CQRS                — Commands y Queries separados
  • Domain-Driven Design (DDD)  — Agregados, Value Objects, Domain Events
  • Microservicios      — 4 servicios independientes con BDs aisladas
  • Event-Driven        — Integración asincrónica vía eventos

LIBRERIAS PRINCIPALES
  
  CQRS y Mediación
    MediatR 12                      — Mediador para CQRS, pipeline behaviors
  
  Validación y Mapeo
    FluentValidation 11             — Validación fluida de reglas de negocio
    AutoMapper 13                   — Mapeo entre DTOs y entidades (VULNERABILIDAD CONOCIDA)
  
  Persistencia
    Entity Framework Core 8          — ORM para .NET
    Npgsql.EntityFrameworkCore.PostgreSQL 8  — Proveedor PostgreSQL
    Microsoft.EntityFrameworkCore.Design 8   — Migraciones y tools
  
  Mensajería Asincrónica
    MassTransit 8                   — Bus de eventos, consumers
    MassTransit.RabbitMQ 8          — Transporte RabbitMQ
  
  Comunicación Sincrónica (gRPC)
    Grpc.AspNetCore 2.60            — Host gRPC en ASP.NET Core
    Grpc.Net.Client 2.60            — Cliente gRPC
    Google.Protobuf 3.25            — Runtime Protocol Buffers
    Grpc.Tools 2.60                 — Generación de código desde .proto
  
  Seguridad y Autenticación
    Microsoft.AspNetCore.Authentication.JwtBearer 8  — Validación JWT
    System.Security.Cryptography   — RSA, SHA256 (built-in .NET)
    BCrypt.Net-Next 4              — Hashing de passwords
  
  API Gateway
    Yarp.ReverseProxy 2.1           — Reverse proxy enrutador
  
  Observabilidad y Trazas
    OpenTelemetry.Extensions.Hosting 1.7              — Hosting
    OpenTelemetry.Instrumentation.AspNetCore 1.7     — Instrumentación web
    OpenTelemetry.Exporter.Jaeger 1.5                — Exporter Jaeger
    Serilog.AspNetCore 8                             — Logging estructurado
  
  API Documentation
    Microsoft.AspNetCore.OpenApi 8  — OpenAPI 3.0 support
    Swashbuckle.AspNetCore 6.4      — Swagger UI y generación
  
  Testing
    xunit 2.7                       — Framework de tests unitarios
    Moq 4.20                        — Mocking de dependencias
    FluentAssertions 6.12           — Assertions fluidas
    Microsoft.AspNetCore.Mvc.Testing 8  — Integration testing
    Testcontainers.PostgreSql 3.7   — PostgreSQL en contenedor para tests
    Testcontainers.RabbitMq 3.7     — RabbitMQ en contenedor para tests
    Microsoft.NET.Test.Sdk 17.9     — SDK de testing
    xunit.runner.visualstudio 2.5   — Test explorer para Visual Studio

INFRAESTRUCTURA Y DEVOPS
  
  Contenedores
    Docker                          — Containerización de cada servicio
    Docker Compose 3.8              — Orquestación local (5 servicios + deps)
  
  Bases de Datos
    PostgreSQL 15                   — RDBMS (4 instancias, una por servicio)
    Npgsql                          — Driver PostgreSQL para .NET
  
  Message Broker
    RabbitMQ                        — Message broker AMQP para eventos
  
  Orquestación
    Kubernetes (K8s)                — Manifests en /k8s/ (para AKS)
    Azure Kubernetes Service (AKS)  — Despliegue en la nube
  
  Observabilidad
    Jaeger                          — Distributed tracing
    OpenTelemetry                   — Instrumentación estándar
    Serilog                         — Logging estructurado
  
  CI/CD
    GitHub Actions                  — Pipelines de integración continua
    Docker Registry                 — Almacenamiento de imágenes

HERRAMIENTAS DE COMPILACIÓN
  • MSBuild               — Sistema de compilación .NET
  • NuGet                 — Gestor de paquetes .NET
  • .NET CLI              — Herramientas de línea de comandos
  • dotnet build          — Compilación de proyectos
  • dotnet test           — Ejecución de tests
  • dotnet publish        — Publicación de binarios
  • dotnet add            — Gestión de dependencias

CONFIGURACIÓN Y GESTIÓN DE VERSIONES
  • Directory.Build.props             — Propiedades de build compartidas
  • Directory.Packages.props          — Central Package Version Management
  • global.json                       — Especificación de SDK .NET 8
  • docker-compose.yml                — Orquestación local
  • MicroservicesSolution.sln         — Solution de Visual Studio


================================================================================
  COMO EJECUTAR
================================================================================

REQUISITOS
  - Docker Desktop instalado y corriendo
  - .NET 8 SDK (para desarrollo local)

LEVANTAR TODO EL ENTORNO
  cd MicroservicesSolution
  docker compose up --build -d

  Los servicios esperan a que PostgreSQL este listo (healthcheck pg_isready)
  antes de arrancar — no hay race conditions.

VERIFICAR QUE TODO ESTE CORRIENDO
  docker compose ps

  Todos los servicios deben mostrar STATUS "Up".

APAGAR TODO
  docker compose down

APAGAR Y BORRAR DATOS (reset completo)
  docker compose down -v


================================================================================
  ESTRUCTURA DE CARPETAS
================================================================================

  MicroservicesSolution/
  |
  +-- src/
  |   +-- Services/
  |   |   +-- Orders/
  |   |   |   +-- Orders.Domain/
  |   |   |   +-- Orders.Application/
  |   |   |   +-- Orders.Infrastructure/
  |   |   |   +-- Orders.API/
  |   |   |   +-- Orders.UnitTests/
  |   |   |   +-- Orders.IntegrationTests/
  |   |   |
  |   |   +-- Products/      (misma estructura)
  |   |   +-- Payments/      (misma estructura)
  |   |   +-- Auth/          (misma estructura)
  |   |
  |   +-- Shared/
  |   |   +-- Common/        Result<T>, AggregateRoot, DomainEvent, IRepository
  |   |   +-- EventBus/      IntegrationEvents compartidos entre servicios
  |   |   +-- Contracts/     Proto definitions gRPC (products.proto)
  |   |
  |   +-- ApiGateway/        YARP reverse proxy
  |
  +-- k8s/
  |   +-- namespaces/        ecommerce-namespace.yaml
  |   +-- deployments/       Un deployment por servicio (3 replicas)
  |   +-- services/          ClusterIP services
  |   +-- ingress/           Ingress para acceso externo
  |   +-- configmaps/        Configuracion por entorno
  |
  +-- .github/
  |   +-- workflows/
  |       +-- orders-ci.yml      Build, test y Docker push
  |       +-- products-ci.yml
  |       +-- payments-ci.yml
  |       +-- auth-ci.yml
  |       +-- deploy-aks.yml     Deploy a Azure Kubernetes Service
  |
  +-- docker-compose.yml     Entorno completo local
  +-- Directory.Packages.props  Versiones NuGet centralizadas
  +-- global.json            Version de SDK (.NET 8)
  +-- MicroservicesSolution.sln


================================================================================
  ESTADO ACTUAL DEL PROYECTO
================================================================================

  COMPLETADO
  [x] Orders Service          — CQRS completo, gRPC client, consumers RabbitMQ
  [x] Products Service        — CQRS completo, gRPC server, EF Core
  [x] Payments Service        — Consumer RabbitMQ, publisher, logica de pago
  [x] Auth Service            — JWT RS256, BCrypt, refresh tokens, JWKS endpoint
  [x] API Gateway             — YARP enrutando los 4 servicios
  [x] Mensajeria asyncrona    — OrderCreated -> PaymentProcessed flujo completo
  [x] gRPC                    — Products expone CheckStock y GetProductDetails
  [x] Docker Compose          — Healthchecks en todas las BDs y RabbitMQ
  [x] Kubernetes manifests    — Deployments, Services, Ingress, ConfigMaps
  [x] GitHub Actions CI/CD    — Pipeline por servicio + deploy a AKS
  [x] Tests Orders            — Unit tests + Integration tests con Testcontainers

  PENDIENTE / MEJORAS POSIBLES
  [ ] Tests Products/Payments/Auth  — proyectos creados, faltan implementaciones
  [ ] [Authorize] en Orders/Products — actualmente sin autenticacion (dev mode)
  [ ] Seed data de productos        — no hay datos iniciales en la BD
  [ ] Middleware en todos los servicios — solo Orders tiene ExceptionHandlingMiddleware
  [ ] Rate limiting en API Gateway  — actualmente solo en Auth Service
  [ ] Renovacion automatica de RSA key — actualmente efimera (nueva clave en cada restart)


================================================================================
  PATRONES Y CONCEPTOS DEMOSTRADOS
================================================================================

  Domain-Driven Design
    - Aggregates con invariantes de negocio (Order, Product, User, Payment)
    - Value Objects (Money, CustomerId, ShippingAddress, PaymentAmount)
    - Domain Events desacoplados de la infraestructura
    - Result<T> pattern — sin excepciones en la capa Application

  CQRS + MediatR
    - Commands (escritura) y Queries (lectura) completamente separados
    - Pipeline Behaviors para cross-cutting concerns (validacion, logging)
    - Cada handler tiene una responsabilidad unica

  Event-Driven Architecture
    - Integration Events via RabbitMQ (entre servicios)
    - Domain Events via MediatR (dentro del servicio)
    - Consumers idempotentes

  Seguridad
    - Criptografia asimetrica RS256 para JWT
    - Defense-in-depth: hashing, rotation, revocacion, rate limiting
    - OWASP: sin SQL injection (EF Core), sin timing attacks, validacion de entrada

  DevOps
    - Imagen Docker multi-stage (build separado del runtime)
    - Healthchecks para startup correcto en Docker y Kubernetes
    - Liveness y readiness probes en K8s
    - Centralized package versioning (Directory.Packages.props)

================================================================================
