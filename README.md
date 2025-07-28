#Electronic-Store
This is a comprehensive e-commerce platform designed for selling electronic products such as laptops, keyboards, headphones, mice, and more. The system focuses on a direct payment process (no online payment gateway) and offers robust management modules for products, orders, and user accounts. It also allows for flexible content management through configurable Filter entities, enabling easy expansion to various product types.

#Features
This platform offers a rich set of features to manage and operate an online electronics store:

Comprehensive Product Management:

Full CRUD (Create, Read, Update, Delete) operations for electronic products.

Flexible product attributes to easily extend and manage diverse product categories.

Order Management:

Create, view, and update order statuses.

Real-time order status updates using SignalR for immediate notifications.

User Account Management:

Secure user registration and login functionalities leveraging JWT (JSON Web Tokens) for authentication.

Email validation and verification using Verifalia API.

Transactional email sending (e.g., confirmations, notifications) via SMTP.

Dynamic Content Management:

Ability to manage and display dynamic page content through flexible Filter entities.

Direct Payment Integration:

Supports an in-person/direct payment process (no online payment gateway integrated).

#Architecture
The project is built upon a modern Microservice Architecture to ensure scalability, independence, and maintainability. Each Microservice adheres to the principles of Clean Architecture, promoting clear separation of concerns and ease of testing.

Ocelot API Gateway: Serves as the single entry point for all frontend requests, routing them efficiently to the appropriate Microservices.

Key Microservices:

Product Service: Manages product information and product attributes.

Order Service: Handles the order processing workflow and order statuses.

User/Auth Service: Manages user accounts, authentication (JWT), and authorization.

Content/Filter Service: Manages Filter entities for dynamic content control.

(Add any other specific services you have, e.g., Notification Service if separate)

Design Patterns: The system extensively utilizes Unit of Work and Repository patterns for effective and maintainable database interactions within each service.

#Technologies Used
This project leverages a modern stack for both backend and frontend development, along with robust database and deployment tools.

Backend (ASP.NET Core .NET 8):

ASP.NET Core .NET 8: Primary framework for all Microservices.

JWT (JSON Web Tokens): For secure user authentication.

SignalR: Enables real-time communication for features like order updates.

Entity Framework Core: Object-Relational Mapper (ORM) for database interactions.

Ocelot: API Gateway for request routing.

Verifalia API: Used for email address verification.

SMTP Client: For sending transactional emails.

(Add other specific libraries like AutoMapper, FluentValidation if used)

Frontend (Angular 19):

Angular 19: The primary framework for the user interface.

Angular Material: UI component library for a modern and responsive design.

#Database:

Microsoft SQL Server: The relational database management system used.

#Tools & Environment:

Docker: Used for containerizing and orchestrating both API services and the frontend application.

Visual Studio 2022 / Visual Studio Code

Swagger UI (for API testing and documentation)
