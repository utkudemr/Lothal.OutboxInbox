# Lothal.OutboxInbox

This project implements the **Inbox-Outbox Pattern** using **Couchbase** and **.NET** in a microservices architecture. This pattern ensures data consistency and reliable messaging between microservices.

> This project is inspired by [Microservices.Tutorial.Outbox.Inbox.Design.Pattern.Example](https://github.com/gncyyldz/Microservices.Tutorial.Outbox.Inbox.Design.Pattern.Example). 

## 📌 Project Goals

- Prevent data loss and ensure reliable message processing with the **Outbox Pattern**.
- Manage duplicate messages and ensure idempotent processing with the **Inbox Pattern**.
- Utilize **Couchbase NoSQL database** for high-performance and scalable data management.
- Integrate with **RabbitMQ** to enable asynchronous messaging.

## 🚀 Technologies Used

- **.NET 8**
- **Couchbase**
- **MassTransit** (for RabbitMQ integration)
- **MediatR**
- **Quartz.NET** (for scheduled jobs)

## 📂 Project Structure

```plaintext
Lothal.OutboxInbox
├── Service.Order.Api          # Handles order creation and writes to the Outbox
├── Service.Order.Publisher    # Reads from the Outbox and publishes messages
├── Service.Order.Consumer     # Processes messages from the Inbox
├── Service.Stock              # Manages stock operations
├── Shared                     # Contains shared models and utilities
└── Lothal.OutboxInbox.sln     # Solution file
```

## 🔧 Setup and Running the Project

### 1️⃣ Requirements

- **.NET 8 SDK**: Required to build and run the project.
- **Couchbase Server**: Used as the database.
- **RabbitMQ**: Used as the message queue.

### 2️⃣ Setting Up Couchbase

You can run Couchbase Server using Docker:

```bash
docker run -d --name couchbase -p 8091-8096:8091-8096 -p 11210:11210 couchbase
```

Then, access the admin panel and configure the necessary **Bucket** and **Collection**.

### 3️⃣ Setting Up RabbitMQ

You can run RabbitMQ using Docker:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

Access the management panel at `http://localhost:15672/`.

### 4️⃣ Running the Project

Run each service in separate terminal windows:

```bash
dotnet run --project Service.Order.Api
```

```bash
dotnet run --project Service.Order.Publisher
```

```bash
dotnet run --project Service.Order.Consumer
```

## 📌 How It Works

1. **Order Creation**: `Service.Order.Api` handles new orders, storing them in the Outbox collection.
2. **Outbox Processing**: `Service.Order.Publisher` periodically scans the Outbox for unprocessed messages and publishes them to the message queue.
3. **Inbox Processing**: `Service.Order.Consumer` consumes the messages, processes them, and ensures idempotency by storing processed message IDs in the Inbox collection.
