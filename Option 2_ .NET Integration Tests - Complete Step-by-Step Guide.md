# Option 2: .NET Integration Tests - Complete Step-by-Step Guide

## 🎯 **Overview: Building Enterprise-Grade Testing**

In this comprehensive guide, I'll walk you through creating a professional .NET testing suite that provides the most thorough validation possible for your Employee Scheduling API. Unlike external testing tools, .NET integration tests run within your application's ecosystem, giving you access to internal components, database state, and business logic validation that external tools cannot provide.

This approach represents the gold standard for API testing in enterprise environments. By the end of this tutorial, you'll have a robust testing framework that validates not just your API endpoints, but also your business logic, data persistence, security implementation, and error handling mechanisms. The testing suite will integrate seamlessly with your development workflow and provide the foundation for continuous integration and deployment pipelines.

The investment in creating this testing infrastructure pays dividends throughout your application's lifecycle. Every code change can be validated automatically, ensuring that new features don't break existing functionality. This level of testing confidence is essential for maintaining a production-ready application and enables rapid, safe development iterations.

## 📋 **Understanding .NET Testing Architecture**

Before diving into implementation, it's crucial to understand the testing architecture we'll be building. This foundation knowledge will help you make informed decisions and understand why each component serves a specific purpose in the overall testing strategy.

### **Testing Pyramid Fundamentals**

The testing pyramid is a fundamental concept in software testing that guides how we structure our test suite. At the base, we have unit tests that validate individual components in isolation. These tests are fast, reliable, and provide immediate feedback about specific functionality. In the middle layer, we have integration tests that validate how components work together, including database interactions, API endpoints, and business workflows. At the top, we have end-to-end tests that validate complete user scenarios across the entire application stack.

For your Employee Scheduling API, we'll focus primarily on integration tests because they provide the best balance of coverage, reliability, and execution speed for API validation. Integration tests can validate your entire request-response cycle, including authentication, authorization, business logic, data persistence, and error handling, while still executing quickly enough for frequent use during development.

### **Test Project Structure and Organization**

A well-organized test project is essential for maintainability and scalability. We'll create a test project that mirrors your API's structure, making it easy to locate and maintain tests as your application grows. The test project will include base classes that provide common functionality, helper methods that simplify test creation, and fixtures that manage test data and database state.

The test organization follows a clear hierarchy that groups related tests together. Authentication tests validate login, registration, and token management. Controller tests validate each API endpoint's behavior under various conditions. Service tests validate business logic and data access patterns. Integration tests validate complete workflows that span multiple components.

### **Database Testing Strategy**

Database testing presents unique challenges because tests must be isolated from each other while still validating real database interactions. We'll implement a strategy that uses a test database with automatic cleanup between tests, ensuring that each test starts with a known state and doesn't interfere with other tests.

The database testing approach includes transaction rollback for fast cleanup, seed data for consistent test conditions, and isolation techniques that prevent tests from affecting each other. This strategy provides confidence that your data access code works correctly while maintaining the speed and reliability necessary for frequent test execution.

## 🚀 **Setting Up the Testing Infrastructure**

The foundation of effective testing is a well-configured testing infrastructure that provides the tools, frameworks, and utilities necessary for comprehensive validation. This section will guide you through creating a robust testing environment that supports all aspects of API testing.

### **Step 1: Create the Test Project**

Creating a dedicated test project separates your testing code from your production code while providing access to internal components for thorough validation. The test project will reference your API project, allowing tests to interact with controllers, services, and other components directly.

**1.1 Create the Test Project Structure:**

Open a command prompt or terminal and navigate to your solution directory. We'll create a new test project that follows .NET testing conventions and integrates seamlessly with your existing solution structure.

```bash
# Navigate to your solution root directory
cd C:\Users\lawry\Downloads\EmployeeScheduling

# Create the test project
dotnet new xunit -n EmployeeScheduling.API.Tests

# Add the test project to your solution
dotnet sln add EmployeeScheduling.API.Tests/EmployeeScheduling.API.Tests.csproj

# Navigate to the test project directory
cd EmployeeScheduling.API.Tests
```

The xUnit framework is the recommended testing framework for .NET applications because of its excellent integration with the .NET ecosystem, comprehensive assertion library, and support for parallel test execution. xUnit provides attributes for organizing tests, fixtures for managing test data, and extensive assertion methods for validating results.

**1.2 Add Required NuGet Packages:**

The testing infrastructure requires several NuGet packages that provide different aspects of testing functionality. Each package serves a specific purpose in creating a comprehensive testing environment.

```bash
# Add core testing packages
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Logging
dotnet add package Newtonsoft.Json
```

Let's understand what each package provides:

Microsoft.AspNetCore.Mvc.Testing enables integration testing by providing a test server that hosts your API in memory. This allows tests to make HTTP requests against your API without requiring a separate server process, making tests faster and more reliable.

Microsoft.EntityFrameworkCore.InMemory provides an in-memory database provider that's perfect for testing. It creates a fast, isolated database for each test run without requiring a real database server, eliminating external dependencies and improving test execution speed.

FluentAssertions provides a more readable and expressive way to write test assertions. Instead of traditional Assert statements, FluentAssertions allows you to write assertions that read like natural language, making tests easier to understand and maintain.

Moq is a mocking framework that allows you to create mock objects for dependencies. This enables you to test components in isolation by replacing their dependencies with controlled mock objects that behave predictably.

**1.3 Add Project References:**

The test project needs references to your API project and shared library to access the components being tested.

```bash
# Add reference to your API project
dotnet add reference ../src/EmployeeScheduling.API/EmployeeScheduling.API.csproj

# Add reference to your shared library
dotnet add reference ../src/EmployeeScheduling.Shared/EmployeeScheduling.Shared.csproj
```

These references allow your test project to access all the public types and members from your API and shared library projects. This access is essential for creating comprehensive tests that validate both public API behavior and internal component interactions.

### **Step 2: Configure the Test Environment**

A properly configured test environment ensures that tests run consistently across different machines and environments. The configuration includes database setup, logging configuration, and service registration that mirrors your production environment while optimizing for testing scenarios.

**2.1 Create Test Configuration Files:**

Create an `appsettings.Test.json` file in your test project root directory. This configuration file will override production settings with values optimized for testing.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=:memory:"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Jwt": {
    "Key": "test-key-that-is-at-least-32-characters-long-for-security",
    "Issuer": "EmployeeScheduling.Test",
    "Audience": "EmployeeScheduling.Test.Users",
    "ExpiryInMinutes": 60
  }
}
```

The test configuration uses an in-memory database connection string that creates a new database for each test run. The logging configuration reduces log verbosity during testing to focus on important messages. The JWT configuration uses test-specific values that don't conflict with production settings.

**2.2 Create Base Test Classes:**

Base test classes provide common functionality that's shared across multiple test classes. This reduces code duplication and ensures consistent test setup and teardown procedures.

Create a file named `TestBase.cs` in your test project:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.Shared.Models;
using System.Net.Http;
using Xunit;

namespace EmployeeScheduling.API.Tests
{
    public class TestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;
        protected readonly IServiceScope _scope;
        protected readonly ApplicationDbContext _context;

        public TestBase(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.Test.json", optional: false);
                });
                
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
                    });
                });
            });

            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created and seeded
            SeedTestData();
        }

        protected virtual void SeedTestData()
        {
            // Create test users
            var adminUser = new User
            {
                Id = 1,
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var testUser = new User
            {
                Id = 2,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                Role = "Employee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.AddRange(adminUser, testUser);

            // Create test employees
            var testEmployee = new Employee
            {
                Id = 1,
                UserId = 2,
                EmployeeNumber = "EMP001",
                FirstName = "Test",
                LastName = "Employee",
                Email = "test.employee@example.com",
                Department = "IT",
                Position = "Developer",
                HourlyRate = 25.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(testEmployee);
            _context.SaveChanges();
        }

        protected async Task<string> GetAuthTokenAsync(string email = "admin@example.com", string password = "Admin123!")
        {
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);
            return loginResponse.token;
        }

        protected void SetAuthorizationHeader(string token)
        {
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _client?.Dispose();
        }
    }
}
```

This base class provides several essential features for testing. The WebApplicationFactory creates an in-memory test server that hosts your API, allowing tests to make real HTTP requests without external dependencies. The test database is created fresh for each test class, ensuring isolation between test runs. The SeedTestData method creates consistent test data that tests can rely on. The authentication helper methods simplify the process of obtaining and using JWT tokens in tests.

### **Step 3: Create Authentication Test Suite**

Authentication is the foundation of API security, so comprehensive authentication testing is essential for ensuring your API properly protects resources and manages user access. This test suite will validate all aspects of authentication, from user registration through token validation and protected resource access.

**3.1 Create Authentication Tests:**

Create a file named `AuthenticationTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace EmployeeScheduling.API.Tests
{
    public class AuthenticationTests : TestBase
    {
        public AuthenticationTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task Register_ValidUser_ReturnsCreated()
        {
            // Arrange
            var registerRequest = new
            {
                Email = "newuser@example.com",
                Password = "NewUser123!",
                ConfirmPassword = "NewUser123!",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            var userResponse = JsonConvert.DeserializeObject<dynamic>(content);
            
            userResponse.Should().NotBeNull();
            ((string)userResponse.email).Should().Be("newuser@example.com");
            ((string)userResponse.firstName).Should().Be("New");
            ((string)userResponse.lastName).Should().Be("User");
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new
            {
                Email = "admin@example.com", // This email already exists in seed data
                Password = "NewUser123!",
                ConfirmPassword = "NewUser123!",
                FirstName = "Duplicate",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenAndUserInfo()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "admin@example.com",
                Password = "Admin123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<dynamic>(content);
            
            loginResponse.Should().NotBeNull();
            ((string)loginResponse.token).Should().NotBeNullOrEmpty();
            ((string)loginResponse.user.email).Should().Be("admin@example.com");
            ((string)loginResponse.user.role).Should().Be("Admin");
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "admin@example.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.GetAsync("/api/employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            SetAuthorizationHeader("invalid-token");

            // Act
            var response = await _client.GetAsync("/api/employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
```

These authentication tests cover the complete authentication workflow, from user registration through protected resource access. Each test validates a specific aspect of authentication behavior, ensuring that your API properly handles both valid and invalid authentication scenarios.

The tests use FluentAssertions to create readable, expressive assertions that clearly communicate the expected behavior. The test methods follow the Arrange-Act-Assert pattern, which separates test setup, execution, and validation into distinct sections that are easy to understand and maintain.

## 🔧 **Creating Comprehensive Controller Tests**

Controller tests validate the behavior of your API endpoints under various conditions, ensuring that they handle valid requests correctly, reject invalid requests appropriately, and return the expected responses. This section will guide you through creating thorough controller tests that cover all your API endpoints.

### **Step 4: Employee Controller Tests**

Employee management is a core feature of your API, so comprehensive testing of employee-related endpoints is essential for ensuring reliable functionality. These tests will validate CRUD operations, search functionality, and business logic related to employee management.

**4.1 Create Employee Controller Tests:**

Create a file named `EmployeeControllerTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Xunit;
using EmployeeScheduling.Shared.DTOs;

namespace EmployeeScheduling.API.Tests
{
    public class EmployeeControllerTests : TestBase
    {
        public EmployeeControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetEmployees_WithValidToken_ReturnsEmployeeList()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.GetAsync("/api/employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var employees = JsonConvert.DeserializeObject<List<dynamic>>(content);
            
            employees.Should().NotBeNull();
            employees.Should().HaveCountGreaterThan(0);
            
            var firstEmployee = employees.First();
            firstEmployee.Should().NotBeNull();
            ((string)firstEmployee.firstName).Should().NotBeNullOrEmpty();
            ((string)firstEmployee.lastName).Should().NotBeNullOrEmpty();
            ((string)firstEmployee.email).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetEmployeeById_ExistingEmployee_ReturnsEmployee()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.GetAsync("/api/employees/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var employee = JsonConvert.DeserializeObject<dynamic>(content);
            
            employee.Should().NotBeNull();
            ((int)employee.id).Should().Be(1);
            ((string)employee.firstName).Should().Be("Test");
            ((string)employee.lastName).Should().Be("Employee");
        }

        [Fact]
        public async Task GetEmployeeById_NonExistentEmployee_ReturnsNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.GetAsync("/api/employees/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateEmployee_ValidData_ReturnsCreatedEmployee()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var createRequest = new
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                EmployeeNumber = "EMP002",
                Department = "HR",
                Position = "Manager",
                HourlyRate = 30.00,
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/employees", createRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            var employee = JsonConvert.DeserializeObject<dynamic>(content);
            
            employee.Should().NotBeNull();
            ((string)employee.firstName).Should().Be("John");
            ((string)employee.lastName).Should().Be("Doe");
            ((string)employee.email).Should().Be("john.doe@example.com");
            ((string)employee.employeeNumber).Should().Be("EMP002");
            ((string)employee.department).Should().Be("HR");
            ((string)employee.position).Should().Be("Manager");
            ((decimal)employee.hourlyRate).Should().Be(30.00m);
            ((bool)employee.isActive).Should().BeTrue();
        }

        [Fact]
        public async Task CreateEmployee_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var createRequest = new
            {
                FirstName = "Duplicate",
                LastName = "User",
                Email = "test.employee@example.com", // This email already exists
                EmployeeNumber = "EMP003",
                Department = "IT",
                Position = "Developer",
                HourlyRate = 25.00,
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/employees", createRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateEmployee_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var createRequest = new
            {
                FirstName = "", // Invalid: empty first name
                LastName = "Doe",
                Email = "invalid-email", // Invalid: not a valid email format
                EmployeeNumber = "EMP004",
                Department = "IT",
                Position = "Developer",
                HourlyRate = -5.00, // Invalid: negative hourly rate
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/employees", createRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateEmployee_ValidData_ReturnsUpdatedEmployee()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var updateRequest = new
            {
                FirstName = "Updated",
                LastName = "Employee",
                Email = "updated.employee@example.com",
                EmployeeNumber = "EMP001",
                Department = "Engineering",
                Position = "Senior Developer",
                HourlyRate = 35.00,
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/employees/1", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var employee = JsonConvert.DeserializeObject<dynamic>(content);
            
            employee.Should().NotBeNull();
            ((string)employee.firstName).Should().Be("Updated");
            ((string)employee.lastName).Should().Be("Employee");
            ((string)employee.department).Should().Be("Engineering");
            ((string)employee.position).Should().Be("Senior Developer");
            ((decimal)employee.hourlyRate).Should().Be(35.00m);
        }

        [Fact]
        public async Task DeleteEmployee_ExistingEmployee_ReturnsNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.DeleteAsync("/api/employees/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify employee is actually deleted
            var getResponse = await _client.GetAsync("/api/employees/1");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteEmployee_NonExistentEmployee_ReturnsNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.DeleteAsync("/api/employees/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
```

These employee controller tests provide comprehensive coverage of all CRUD operations and edge cases. Each test validates a specific scenario, from successful operations with valid data to error handling with invalid inputs. The tests ensure that your employee management endpoints behave correctly under all conditions.

### **Step 5: Schedule Controller Tests**

Schedule management is another critical component that requires thorough testing to ensure business logic works correctly and data integrity is maintained. These tests will validate schedule creation, modification, publishing workflows, and date-based filtering.

**5.1 Create Schedule Controller Tests:**

Create a file named `ScheduleControllerTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Xunit;

namespace EmployeeScheduling.API.Tests
{
    public class ScheduleControllerTests : TestBase
    {
        public ScheduleControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        protected override void SeedTestData()
        {
            base.SeedTestData();

            // Add test schedule data
            var testSchedule = new EmployeeScheduling.Shared.Models.Schedule
            {
                Id = 1,
                Name = "Test Schedule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(6),
                IsPublished = false,
                Description = "Test schedule for integration testing",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            _context.Schedules.Add(testSchedule);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetSchedules_WithValidToken_ReturnsScheduleList()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.GetAsync("/api/schedules");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var schedules = JsonConvert.DeserializeObject<List<dynamic>>(content);
            
            schedules.Should().NotBeNull();
            schedules.Should().HaveCountGreaterThan(0);
            
            var firstSchedule = schedules.First();
            firstSchedule.Should().NotBeNull();
            ((string)firstSchedule.name).Should().NotBeNullOrEmpty();
            ((DateTime)firstSchedule.startDate).Should().BeAfter(DateTime.MinValue);
            ((DateTime)firstSchedule.endDate).Should().BeAfter(DateTime.MinValue);
        }

        [Fact]
        public async Task CreateSchedule_ValidData_ReturnsCreatedSchedule()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var createRequest = new
            {
                Name = "New Test Schedule",
                StartDate = DateTime.UtcNow.Date.AddDays(7),
                EndDate = DateTime.UtcNow.Date.AddDays(13),
                Description = "A new schedule for testing",
                IsPublished = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/schedules", createRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            var schedule = JsonConvert.DeserializeObject<dynamic>(content);
            
            schedule.Should().NotBeNull();
            ((string)schedule.name).Should().Be("New Test Schedule");
            ((string)schedule.description).Should().Be("A new schedule for testing");
            ((bool)schedule.isPublished).Should().BeFalse();
        }

        [Fact]
        public async Task CreateSchedule_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var createRequest = new
            {
                Name = "Invalid Schedule",
                StartDate = DateTime.UtcNow.Date.AddDays(7),
                EndDate = DateTime.UtcNow.Date.AddDays(3), // End date before start date
                Description = "Invalid schedule with bad date range",
                IsPublished = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/schedules", createRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PublishSchedule_UnpublishedSchedule_ReturnsPublishedSchedule()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Act
            var response = await _client.PostAsync("/api/schedules/1/publish", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var schedule = JsonConvert.DeserializeObject<dynamic>(content);
            
            schedule.Should().NotBeNull();
            ((bool)schedule.isPublished).Should().BeTrue();
        }

        [Fact]
        public async Task UnpublishSchedule_PublishedSchedule_ReturnsUnpublishedSchedule()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // First publish the schedule
            await _client.PostAsync("/api/schedules/1/publish", null);

            // Act
            var response = await _client.PostAsync("/api/schedules/1/unpublish", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var schedule = JsonConvert.DeserializeObject<dynamic>(content);
            
            schedule.Should().NotBeNull();
            ((bool)schedule.isPublished).Should().BeFalse();
        }

        [Fact]
        public async Task GetSchedulesByDateRange_ValidRange_ReturnsFilteredSchedules()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var startDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.Date.AddDays(10).ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/schedules?startDate={startDate}&endDate={endDate}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var schedules = JsonConvert.DeserializeObject<List<dynamic>>(content);
            
            schedules.Should().NotBeNull();
            
            // Verify all returned schedules fall within the requested date range
            foreach (var schedule in schedules)
            {
                var scheduleStartDate = DateTime.Parse((string)schedule.startDate);
                var scheduleEndDate = DateTime.Parse((string)schedule.endDate);
                
                scheduleStartDate.Should().BeOnOrAfter(DateTime.Parse(startDate));
                scheduleEndDate.Should().BeOnOrBefore(DateTime.Parse(endDate));
            }
        }
    }
}
```

The schedule controller tests validate the complete schedule management workflow, including creation, modification, publishing state changes, and date-based filtering. These tests ensure that your schedule management logic works correctly and maintains data integrity throughout all operations.

## 📊 **Advanced Testing Scenarios**

Beyond basic CRUD operations, comprehensive API testing must validate complex business scenarios, error handling, performance characteristics, and security measures. This section covers advanced testing techniques that ensure your API behaves correctly under all conditions.

### **Step 6: Business Logic Testing**

Business logic testing validates that your API correctly implements the rules and workflows that define your application's behavior. These tests go beyond simple data validation to ensure that complex business scenarios work as expected.

**6.1 Create Business Logic Tests:**

Create a file named `BusinessLogicTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Xunit;
using EmployeeScheduling.Shared.Models;

namespace EmployeeScheduling.API.Tests
{
    public class BusinessLogicTests : TestBase
    {
        public BusinessLogicTests(WebApplicationFactory<Program> factory) : base(factory) { }

        protected override void SeedTestData()
        {
            base.SeedTestData();

            // Add comprehensive test data for business logic testing
            var schedule = new Schedule
            {
                Id = 1,
                Name = "Business Logic Test Schedule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(6),
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            var shift1 = new Shift
            {
                Id = 1,
                ScheduleId = 1,
                Title = "Morning Shift",
                StartTime = DateTime.UtcNow.Date.AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddHours(16),
                RequiredEmployees = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            var shift2 = new Shift
            {
                Id = 2,
                ScheduleId = 1,
                Title = "Evening Shift",
                StartTime = DateTime.UtcNow.Date.AddHours(16),
                EndTime = DateTime.UtcNow.Date.AddHours(24),
                RequiredEmployees = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            _context.Schedules.Add(schedule);
            _context.Shifts.AddRange(shift1, shift2);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AssignEmployeeToShift_ValidAssignment_ReturnsSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var assignmentRequest = new
            {
                EmployeeId = 1,
                ShiftId = 1,
                Status = "Assigned"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assignments", assignmentRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            var assignment = JsonConvert.DeserializeObject<dynamic>(content);
            
            assignment.Should().NotBeNull();
            ((int)assignment.employeeId).Should().Be(1);
            ((int)assignment.shiftId).Should().Be(1);
            ((string)assignment.status).Should().Be("Assigned");
        }

        [Fact]
        public async Task AssignEmployeeToOverlappingShift_ShouldPreventDoubleBooking()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // First assignment
            var firstAssignment = new
            {
                EmployeeId = 1,
                ShiftId = 1,
                Status = "Assigned"
            };

            await _client.PostAsJsonAsync("/api/assignments", firstAssignment);

            // Attempt to assign same employee to overlapping shift
            var overlappingAssignment = new
            {
                EmployeeId = 1,
                ShiftId = 2, // This shift overlaps with shift 1
                Status = "Assigned"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/assignments", overlappingAssignment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("conflict").Or.Contain("overlap").Or.Contain("double");
        }

        [Fact]
        public async Task CalculatePayroll_ForCompletedShifts_ReturnsAccurateCalculation()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Create assignment with check-in and check-out times
            var assignment = new
            {
                EmployeeId = 1,
                ShiftId = 1,
                Status = "Assigned"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/assignments", assignment);
            var assignmentContent = await createResponse.Content.ReadAsStringAsync();
            var createdAssignment = JsonConvert.DeserializeObject<dynamic>(assignmentContent);
            var assignmentId = (int)createdAssignment.id;

            // Check in
            var checkInRequest = new
            {
                CheckInTime = DateTime.UtcNow.Date.AddHours(8),
                Notes = "Checked in on time"
            };

            await _client.PostAsJsonAsync($"/api/assignments/{assignmentId}/checkin", checkInRequest);

            // Check out
            var checkOutRequest = new
            {
                CheckOutTime = DateTime.UtcNow.Date.AddHours(16),
                Notes = "Completed shift"
            };

            await _client.PostAsJsonAsync($"/api/assignments/{assignmentId}/checkout", checkOutRequest);

            // Act - Calculate payroll
            var payrollResponse = await _client.GetAsync($"/api/assignments/{assignmentId}/payroll");

            // Assert
            payrollResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var payrollContent = await payrollResponse.Content.ReadAsStringAsync();
            var payroll = JsonConvert.DeserializeObject<dynamic>(payrollContent);
            
            payroll.Should().NotBeNull();
            ((decimal)payroll.regularHours).Should().Be(8.0m);
            ((decimal)payroll.overtimeHours).Should().Be(0.0m);
            ((decimal)payroll.totalPay).Should().Be(200.0m); // 8 hours * $25/hour
        }

        [Fact]
        public async Task CalculatePayroll_WithOvertime_ReturnsCorrectOvertimeCalculation()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Create assignment
            var assignment = new
            {
                EmployeeId = 1,
                ShiftId = 1,
                Status = "Assigned"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/assignments", assignment);
            var assignmentContent = await createResponse.Content.ReadAsStringAsync();
            var createdAssignment = JsonConvert.DeserializeObject<dynamic>(assignmentContent);
            var assignmentId = (int)createdAssignment.id;

            // Check in
            var checkInRequest = new
            {
                CheckInTime = DateTime.UtcNow.Date.AddHours(8),
                Notes = "Checked in on time"
            };

            await _client.PostAsJsonAsync($"/api/assignments/{assignmentId}/checkin", checkInRequest);

            // Check out after 10 hours (2 hours overtime)
            var checkOutRequest = new
            {
                CheckOutTime = DateTime.UtcNow.Date.AddHours(18),
                Notes = "Worked overtime"
            };

            await _client.PostAsJsonAsync($"/api/assignments/{assignmentId}/checkout", checkOutRequest);

            // Act - Calculate payroll
            var payrollResponse = await _client.GetAsync($"/api/assignments/{assignmentId}/payroll");

            // Assert
            payrollResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var payrollContent = await payrollResponse.Content.ReadAsStringAsync();
            var payroll = JsonConvert.DeserializeObject<dynamic>(payrollContent);
            
            payroll.Should().NotBeNull();
            ((decimal)payroll.regularHours).Should().Be(8.0m);
            ((decimal)payroll.overtimeHours).Should().Be(2.0m);
            // Regular: 8 * $25 = $200, Overtime: 2 * $37.50 = $75, Total: $275
            ((decimal)payroll.totalPay).Should().Be(275.0m);
        }

        [Fact]
        public async Task GetAvailableEmployees_ForShift_ReturnsOnlyAvailableEmployees()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            // Set employee availability
            var availabilityRequest = new
            {
                EmployeeId = 1,
                DayOfWeek = (int)DateTime.UtcNow.DayOfWeek,
                StartTime = "08:00",
                EndTime = "17:00",
                AvailabilityType = "Available"
            };

            await _client.PostAsJsonAsync("/api/availability", availabilityRequest);

            // Act
            var response = await _client.GetAsync("/api/shifts/1/available-employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var availableEmployees = JsonConvert.DeserializeObject<List<dynamic>>(content);
            
            availableEmployees.Should().NotBeNull();
            availableEmployees.Should().HaveCountGreaterThan(0);
            
            // Verify that returned employees are actually available for the shift time
            foreach (var employee in availableEmployees)
            {
                ((bool)employee.isAvailable).Should().BeTrue();
            }
        }
    }
}
```

These business logic tests validate complex scenarios that involve multiple components working together. They ensure that your API correctly implements business rules like preventing double-booking, calculating payroll accurately, and matching available employees to shifts.

### **Step 7: Performance and Load Testing**

Performance testing ensures that your API can handle expected load levels and responds within acceptable time limits. These tests help identify bottlenecks and ensure that your API remains responsive under stress.

**7.1 Create Performance Tests:**

Create a file named `PerformanceTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Xunit;

namespace EmployeeScheduling.API.Tests
{
    public class PerformanceTests : TestBase
    {
        public PerformanceTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetEmployees_ResponseTime_ShouldBeFast()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var response = await _client.GetAsync("/api/employees");
            stopwatch.Stop();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should respond within 1 second
        }

        [Fact]
        public async Task ConcurrentRequests_ShouldHandleMultipleUsers()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            var tasks = new List<Task<HttpResponseMessage>>();

            // Create multiple concurrent requests
            for (int i = 0; i < 10; i++)
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                tasks.Add(client.GetAsync("/api/employees"));
            }

            // Act
            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().HaveCount(10);
            responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task BulkOperations_ShouldHandleLargeDataSets()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthorizationHeader(token);

            var bulkEmployees = new List<object>();
            for (int i = 0; i < 100; i++)
            {
                bulkEmployees.Add(new
                {
                    FirstName = $"Employee{i}",
                    LastName = $"Test{i}",
                    Email = $"employee{i}@example.com",
                    EmployeeNumber = $"EMP{i:000}",
                    Department = "IT",
                    Position = "Developer",
                    HourlyRate = 25.00,
                    IsActive = true
                });
            }

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var response = await _client.PostAsJsonAsync("/api/employees/bulk", bulkEmployees);
            stopwatch.Stop();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
        }
    }
}
```

Performance tests help ensure that your API meets response time requirements and can handle concurrent users effectively. These tests establish baseline performance metrics and help identify performance regressions as your application evolves.

## 🚀 **Running and Maintaining Your Test Suite**

A comprehensive test suite is only valuable if it's easy to run, maintain, and integrate into your development workflow. This section covers best practices for executing tests, interpreting results, and maintaining test quality over time.

### **Step 8: Test Execution and Reporting**

**8.1 Run All Tests:**

Execute your complete test suite using the .NET CLI:

```bash
# Navigate to your test project directory
cd EmployeeScheduling.API.Tests

# Run all tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests and generate detailed reports
dotnet test --logger "trx;LogFileName=test-results.trx" --logger "html;LogFileName=test-results.html"
```

**8.2 Generate Coverage Reports:**

Install the coverage report generator and create detailed coverage reports:

```bash
# Install the report generator tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html

# Open the coverage report
start coverage-report/index.html  # Windows
open coverage-report/index.html   # macOS
```

**8.3 Continuous Integration Setup:**

Create a GitHub Actions workflow file `.github/workflows/test.yml`:

```yaml
name: API Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Generate coverage report
      run: |
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
        
    - name: Upload coverage reports
      uses: actions/upload-artifact@v3
      with:
        name: coverage-report
        path: coverage-report/
```

This workflow automatically runs your tests on every push and pull request, ensuring that code changes don't break existing functionality.

### **Step 9: Test Maintenance and Best Practices**

**9.1 Test Organization Principles:**

Organize your tests following these principles to maintain clarity and ease of maintenance:

- **One test class per controller or service** to keep related tests together
- **Descriptive test method names** that clearly indicate what is being tested
- **Arrange-Act-Assert pattern** for consistent test structure
- **Test data isolation** to prevent tests from affecting each other
- **Meaningful assertions** that validate the specific behavior being tested

**9.2 Common Testing Patterns:**

Implement these patterns to improve test quality and maintainability:

```csharp
// Factory pattern for creating test data
public static class TestDataFactory
{
    public static Employee CreateTestEmployee(string email = null)
    {
        return new Employee
        {
            FirstName = "Test",
            LastName = "Employee",
            Email = email ?? $"test{Guid.NewGuid()}@example.com",
            EmployeeNumber = $"EMP{Random.Shared.Next(1000, 9999)}",
            Department = "IT",
            Position = "Developer",
            HourlyRate = 25.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Builder pattern for complex test scenarios
public class ScheduleTestBuilder
{
    private Schedule _schedule = new Schedule();

    public ScheduleTestBuilder WithName(string name)
    {
        _schedule.Name = name;
        return this;
    }

    public ScheduleTestBuilder WithDateRange(DateTime start, DateTime end)
    {
        _schedule.StartDate = start;
        _schedule.EndDate = end;
        return this;
    }

    public ScheduleTestBuilder Published()
    {
        _schedule.IsPublished = true;
        return this;
    }

    public Schedule Build() => _schedule;
}
```

**9.3 Test Data Management:**

Implement strategies for managing test data effectively:

```csharp
// Database seeding for consistent test data
public class TestDatabaseSeeder
{
    public static void SeedDatabase(ApplicationDbContext context)
    {
        if (context.Users.Any()) return; // Already seeded

        var users = new[]
        {
            new User { Email = "admin@test.com", Role = "Admin", /* ... */ },
            new User { Email = "employee@test.com", Role = "Employee", /* ... */ }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}

// Test data cleanup
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        TestDatabaseSeeder.SeedDatabase(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
```

## 🎉 **Congratulations! You've Completed Option 2**

You now have a comprehensive .NET integration testing suite that provides enterprise-grade validation for your Employee Scheduling API. This testing infrastructure offers several significant advantages over external testing tools:

**Deep Integration Testing:** Your tests can access internal components, validate business logic implementation, and ensure data persistence works correctly. This level of testing is impossible with external tools that only see the API surface.

**Development Workflow Integration:** The tests integrate seamlessly with your .NET development environment, running in the same ecosystem as your production code. This integration enables advanced debugging, profiling, and analysis capabilities.

**Continuous Integration Ready:** The test suite is designed for automated execution in CI/CD pipelines, providing immediate feedback on code changes and preventing regressions from reaching production.

**Comprehensive Coverage:** The tests validate not just API endpoints, but also business logic, data access patterns, security implementation, and performance characteristics. This comprehensive coverage provides confidence that your entire application stack works correctly.

**Maintainable and Scalable:** The test architecture is designed for long-term maintenance and growth. As your API evolves, the testing framework can easily accommodate new features and requirements.

Your testing suite now provides the foundation for confident, rapid development. Every code change can be validated automatically, ensuring that new features don't break existing functionality while maintaining the high quality standards expected in production environments.

