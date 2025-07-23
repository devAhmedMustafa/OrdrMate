# OrdrMate System Documentation

This directory contains comprehensive Mermaid documentation for the OrdrMate restaurant management system. The documentation is organized to follow developer standards and provide clear insights into the system architecture, data flow, and component relationships.

## Documentation Structure

### 1. Architecture Overview
- **System Architecture Diagram** - High-level view of the entire OrdrMate system
- **Component Architecture** - Detailed breakdown of system components and their interactions

### 2. Data Models
- **Entity Relationship Diagram (ERD)** - Complete database schema and relationships
- **Domain Model** - Core business entities and their relationships
- **Class Diagrams** - Detailed class structures for key components

### 3. Business Flows
- **Order Management Flow** - Complete order lifecycle from placement to completion
- **User Authentication Flow** - Authentication and authorization processes
- **Table Reservation Flow** - Table booking and management process
- **Kitchen Operations Flow** - Food preparation and queue management

### 4. API Documentation
- **API Architecture** - REST API structure and endpoints
- **Service Layer Architecture** - Business logic organization

### 5. Deployment & Infrastructure
- **System Deployment** - Deployment architecture and infrastructure setup

## Key Features Documented

The OrdrMate system manages:

- **Multi-tenant Restaurant Management** - Support for multiple restaurants with branches
- **Order Management** - Both dine-in and takeaway orders
- **Kitchen Operations** - Kitchen queues, food preparation tracking
- **Table Reservations** - Table booking and waiting queue management
- **Payment Processing** - Integration with payment providers (Paymob)
- **Real-time Updates** - WebSocket-based live updates
- **User Management** - Role-based access (Admin, Restaurant Manager, Branch Manager, Customer)

## Technology Stack

- **Backend**: ASP.NET Core Web API
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT-based authentication
- **Real-time Communication**: WebSockets
- **Payment Integration**: Paymob payment gateway
- **Cloud Services**: AWS S3 for file storage
- **Push Notifications**: Firebase Cloud Messaging

## How to Use This Documentation

1. Start with the **System Architecture** to understand the overall system design
2. Review the **ERD** to understand data relationships
3. Follow specific **Business Flow** diagrams for detailed process understanding
4. Refer to **Class Diagrams** for implementation details
5. Use **API Architecture** for integration and development

Each diagram includes detailed annotations and follows Mermaid best practices for clarity and maintainability.
