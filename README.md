# 📚 Librow - Library Management System

## 📝 Project Overview

**Librow** is a robust Library Management System built with **ASP.NET Core** for the R2E midterm project at NashTech. It streamlines library operations by enabling:

- **Book and Category Management**: Admins can add, update, and delete books and categories.
- **User Role Management**: Supports Admin and Customer roles with distinct permissions.
- **Borrowing Workflow**: Users can borrow books, track statuses (Approved/Rejected/Waiting), and receive email notifications on status changes.
- **Book Ratings and Reviews**: Customers can rate and review books after borrowing.
- **Search and Filter**: Easily find books by title, author, or category, with filtering by availability, ratings, and categories.
- **Admin Dashboard**: Provides statistics on borrowings, popular categories, and user activities with visual graphs.

The system ensures secure access with JWT-based authentication, uses Entity Framework Core for database operations, and includes unit tests for reliability.

## 🛠️ Technologies Used

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQL Server
- xUnit & Moq (Unit Testing)
- Swagger (API UI Testing)
- ....

## 🚀 How to Clone and Run the Project

### 1. Clone the Repository

```bash
git clone https://github.com/hugnt/NT-MA-LibruaryManagement-Librow-BE.git
cd NT-MA-LibruaryManagement-Librow-BE
```

### 2. Run the Application

```bash
dotnet run --project Librow.API
```

The API will be available at:

- `http://localhost:5255`

Swagger UI: `http://localhost:5255/swagger`

## 🔐 Test Accounts

Use the following test accounts to log in and test features:

| Role     | Usename | Password |
| -------- | ------- | -------- |
| Admin    | admin   | 123456   |
| Customer | user001 | 123456   |

✅ **Admin**: Manage users, books, categories, approve/decline borrow requests  
✅ **Customer**: Search books, send borrow requests, rate borrowed books

## ✅ Key Features

- [x] Role-based authentication and authorization
- [x] Book & category management (Admin only)
- [x] Borrow request & approval workflow
- [x] Book rating after return
- [x] Swagger for API testing
- [x] Email notifications for borrowing status changes
- [x] Search and filter books by title, author, category, availability, and ratings
- [x] Admin dashboard with borrowing statistics and visual graphs

## 📬 Contact

For any questions or feedback, please contact: **thanh.hung.st302@gmail.com**
