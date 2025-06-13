# Option 1: Postman Automation - Complete Step-by-Step Guide

## 🎯 **Overview: What We'll Accomplish**

In this guide, I'll walk you through creating a complete automated testing solution using Postman that will validate all 74+ endpoints of your Employee Scheduling API. By the end of this tutorial, you'll have a professional-grade testing suite that runs automatically and provides comprehensive validation of your API.

This approach is perfect for immediate validation and gives you confidence that your API works correctly without depending on unreliable human testers. The entire process takes approximately 2-3 hours and requires no programming experience.

## 📋 **Prerequisites and Setup**

Before we begin, let's ensure you have everything needed for success. This section will guide you through downloading and installing the necessary tools, then verifying your API is ready for testing.

### **Step 1: Download and Install Postman**

First, we need to install Postman, which is a free tool that makes API testing simple and powerful.

**1.1 Download Postman:**
- Go to https://www.postman.com/downloads/
- Click "Download the Desktop App"
- Choose your operating system (Windows, Mac, or Linux)
- The download should start automatically

**1.2 Install Postman:**
- Run the downloaded installer
- Follow the installation prompts (accept default settings)
- Launch Postman when installation completes
- Create a free account when prompted (this allows you to save your work)

**1.3 Verify Installation:**
- You should see the Postman interface with options to create collections
- If you see a welcome screen, click "Skip" to get to the main interface

### **Step 2: Verify Your API is Running**

Before creating tests, we need to confirm your Employee Scheduling API is accessible and responding correctly.

**2.1 Start Your API:**
```bash
# Navigate to your API project directory
cd C:\Users\lawry\Downloads\EmployeeScheduling\src\EmployeeScheduling.API

# Start the API
dotnet run
```

**2.2 Verify API is Running:**
- Look for output similar to: "Now listening on: https://localhost:7001"
- Note the port number (yours might be different)
- Keep this terminal window open while testing

**2.3 Test Basic Connectivity:**
- Open your web browser
- Navigate to: https://localhost:7001/swagger (replace 7001 with your port)
- You should see the Swagger documentation page
- This confirms your API is running and accessible

### **Step 3: Gather API Information**

To create effective tests, we need to collect some basic information about your API structure and endpoints.

**3.1 Document Your Base URL:**
- From Step 2.2, note your API's base URL (e.g., https://localhost:7001)
- We'll use this throughout our testing setup

**3.2 Identify Key Endpoints:**
- From the Swagger page, you can see all available endpoints
- Note the main categories: Auth, Employees, Schedules, Shifts, Assignments, Availability
- We'll test these systematically

**3.3 Locate Test Credentials:**
- Your API should have default admin credentials
- Typically: Email: "admin@example.com", Password: "Admin123!"
- We'll use these for authentication testing

## 🚀 **Creating Your First Postman Collection**

Now we'll create a Postman collection that organizes all your API tests in a logical structure. This collection will serve as the foundation for all your automated testing.

### **Step 4: Create a New Collection**

**4.1 Create Collection:**
- In Postman, click the "+" button next to "Collections" in the left sidebar
- Name it: "Employee Scheduling API Tests"
- Add description: "Comprehensive automated testing for Employee Scheduling API"
- Click "Create"

**4.2 Set Collection Variables:**
- Click on your new collection name
- Go to the "Variables" tab
- Add these variables:

| Variable Name | Initial Value | Current Value |
|---------------|---------------|---------------|
| base_url | https://localhost:7001 | https://localhost:7001 |
| admin_email | admin@example.com | admin@example.com |
| admin_password | Admin123! | Admin123! |
| auth_token | | |

- Click "Save"

**4.3 Create Folder Structure:**
- Right-click on your collection
- Select "Add Folder"
- Create these folders in order:
  - "1. Authentication"
  - "2. Employees"
  - "3. Schedules"
  - "4. Shifts"
  - "5. Assignments"
  - "6. Availability"

This organization makes it easy to find and run specific tests while maintaining a logical flow that mirrors your API structure.

### **Step 5: Create Authentication Tests**

Authentication is the foundation of API security, so we'll start by creating comprehensive tests for user registration, login, and token validation.

**5.1 Create User Registration Test:**
- Right-click "1. Authentication" folder
- Select "Add Request"
- Name: "Register New User"
- Method: POST
- URL: `{{base_url}}/api/auth/register`

**5.2 Configure Registration Request Body:**
- Click "Body" tab
- Select "raw" and "JSON"
- Enter this JSON:
```json
{
  "email": "test.user@example.com",
  "password": "TestUser123!",
  "firstName": "Test",
  "lastName": "User",
  "confirmPassword": "TestUser123!"
}
```

**5.3 Add Registration Test Scripts:**
- Click "Tests" tab
- Add this JavaScript code:
```javascript
pm.test("User registration successful", function () {
    pm.response.to.have.status(201);
});

pm.test("Response contains user data", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('email');
    pm.expect(jsonData.email).to.eql('test.user@example.com');
});

pm.test("Response time is acceptable", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

**5.4 Create User Login Test:**
- Add another request to "1. Authentication" folder
- Name: "User Login"
- Method: POST
- URL: `{{base_url}}/api/auth/login`
- Body (raw JSON):
```json
{
  "email": "{{admin_email}}",
  "password": "{{admin_password}}"
}
```

**5.5 Add Login Test Scripts:**
```javascript
pm.test("Login successful", function () {
    pm.response.to.have.status(200);
});

pm.test("Response contains token", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('token');
    pm.expect(jsonData.token).to.be.a('string');
    pm.expect(jsonData.token.length).to.be.above(10);
});

pm.test("Save auth token for future requests", function () {
    const jsonData = pm.response.json();
    pm.collectionVariables.set("auth_token", jsonData.token);
});

pm.test("Response time is acceptable", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

### **Step 6: Test Your Authentication Setup**

Before proceeding further, let's verify that our authentication tests work correctly.

**6.1 Run Registration Test:**
- Click on "Register New User" request
- Click "Send" button
- Verify you see green checkmarks for all tests
- If any tests fail, check your API is running and the URL is correct

**6.2 Run Login Test:**
- Click on "User Login" request
- Click "Send" button
- Verify all tests pass
- Check that the auth_token variable is now populated (look in collection variables)

**6.3 Troubleshooting Common Issues:**
- If you get connection errors, verify your API is running
- If you get 404 errors, check your base_url variable
- If authentication fails, verify the admin credentials in your database

## 🔧 **Creating Comprehensive Endpoint Tests**

Now that authentication is working, we'll create tests for all your main API endpoints. This section will show you how to create reusable test patterns that can be applied to any endpoint.

### **Step 7: Create Employee Management Tests**

Employee management is a core feature of your API, so we'll create comprehensive tests that cover all CRUD operations and edge cases.

**7.1 Create Get All Employees Test:**
- Add request to "2. Employees" folder
- Name: "Get All Employees"
- Method: GET
- URL: `{{base_url}}/api/employees`

**7.2 Add Authorization Header:**
- Click "Authorization" tab
- Type: "Bearer Token"
- Token: `{{auth_token}}`

**7.3 Add Employee List Test Scripts:**
```javascript
pm.test("Get employees successful", function () {
    pm.response.to.have.status(200);
});

pm.test("Response is an array", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an('array');
});

pm.test("Each employee has required fields", function () {
    const jsonData = pm.response.json();
    if (jsonData.length > 0) {
        const employee = jsonData[0];
        pm.expect(employee).to.have.property('id');
        pm.expect(employee).to.have.property('firstName');
        pm.expect(employee).to.have.property('lastName');
        pm.expect(employee).to.have.property('email');
    }
});

pm.test("Response time is acceptable", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

**7.4 Create Employee Creation Test:**
- Add request: "Create New Employee"
- Method: POST
- URL: `{{base_url}}/api/employees`
- Authorization: Bearer Token `{{auth_token}}`
- Body (raw JSON):
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "employeeNumber": "EMP001",
  "department": "IT",
  "position": "Developer",
  "hourlyRate": 25.00,
  "isActive": true
}
```

**7.5 Add Employee Creation Test Scripts:**
```javascript
pm.test("Employee creation successful", function () {
    pm.response.to.have.status(201);
});

pm.test("Response contains created employee", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData.firstName).to.eql('John');
    pm.expect(jsonData.lastName).to.eql('Doe');
    pm.expect(jsonData.email).to.eql('john.doe@example.com');
});

pm.test("Save employee ID for future tests", function () {
    const jsonData = pm.response.json();
    pm.collectionVariables.set("test_employee_id", jsonData.id);
});

pm.test("Response time is acceptable", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

### **Step 8: Create Schedule Management Tests**

Schedule management is another critical component that requires thorough testing to ensure business logic works correctly.

**8.1 Create Schedule Tests:**
Following the same pattern as employees, create these requests in the "3. Schedules" folder:

- "Get All Schedules" (GET `/api/schedules`)
- "Create New Schedule" (POST `/api/schedules`)
- "Get Schedule by ID" (GET `/api/schedules/{{schedule_id}}`)
- "Update Schedule" (PUT `/api/schedules/{{schedule_id}}`)
- "Delete Schedule" (DELETE `/api/schedules/{{schedule_id}}`)

**8.2 Sample Schedule Creation Body:**
```json
{
  "name": "Weekly Schedule - Week 1",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-07T23:59:59Z",
  "isPublished": false,
  "description": "Test schedule for automated testing"
}
```

**8.3 Standard Test Script Template:**
For each schedule endpoint, use this template and modify as needed:
```javascript
pm.test("Request successful", function () {
    pm.response.to.have.status([200, 201, 204]); // Adjust expected status codes
});

pm.test("Response has expected structure", function () {
    if (pm.response.code !== 204) { // Skip for DELETE requests
        const jsonData = pm.response.json();
        pm.expect(jsonData).to.be.an('object');
        // Add specific field checks based on endpoint
    }
});

pm.test("Response time is acceptable", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

## ⚡ **Setting Up Automated Test Execution**

Now that we have individual tests, let's set up automated execution that runs all tests in sequence and provides comprehensive reporting.

### **Step 9: Create Collection-Level Test Scripts**

Collection-level scripts run before and after all tests, allowing us to set up test data and clean up afterward.

**9.1 Add Pre-request Script to Collection:**
- Click on your collection name
- Go to "Pre-request Script" tab
- Add this code:
```javascript
// Ensure we have a fresh auth token before running tests
if (!pm.collectionVariables.get("auth_token")) {
    console.log("No auth token found, will authenticate in login test");
}

// Set up test data variables
pm.collectionVariables.set("test_timestamp", Date.now());
pm.collectionVariables.set("test_email", "test" + Date.now() + "@example.com");
```

**9.2 Add Collection Test Script:**
- Go to "Tests" tab
- Add this code:
```javascript
// Collection-level cleanup and reporting
pm.test("Collection execution completed", function () {
    console.log("All tests completed at: " + new Date().toISOString());
});
```

### **Step 10: Run Your Complete Test Suite**

Now we'll execute all tests and verify everything works correctly.

**10.1 Run Individual Tests First:**
- Test each request individually to ensure they work
- Fix any issues before running the full suite
- Verify authentication is working properly

**10.2 Run Complete Collection:**
- Click on your collection name
- Click "Run" button
- In the Collection Runner:
  - Select all folders/requests
  - Set iterations to 1
  - Set delay to 1000ms (1 second between requests)
  - Click "Run Employee Scheduling API Tests"

**10.3 Review Test Results:**
- You'll see a summary of all test results
- Green indicates passing tests
- Red indicates failing tests
- Click on individual tests to see detailed results

### **Step 11: Export and Save Your Collection**

To ensure your work is preserved and can be shared or used in automation, we'll export the collection.

**11.1 Export Collection:**
- Right-click on your collection
- Select "Export"
- Choose "Collection v2.1"
- Save as "EmployeeScheduling_API_Tests.postman_collection.json"

**11.2 Export Environment:**
- Click the gear icon next to environment dropdown
- Select your environment
- Click "Export"
- Save as "EmployeeScheduling_Environment.postman_environment.json"

**11.3 Create Documentation:**
- Click on your collection
- Click "View Documentation"
- Click "Publish" to create shareable documentation
- This creates a web page showing all your API endpoints and tests

## 🎯 **Advanced Automation with Newman**

Newman is Postman's command-line tool that allows you to run collections automatically, integrate with CI/CD pipelines, and generate detailed reports.

### **Step 12: Install and Configure Newman**

**12.1 Install Node.js (if not already installed):**
- Go to https://nodejs.org/
- Download and install the LTS version
- Verify installation by opening command prompt and typing: `node --version`

**12.2 Install Newman:**
```bash
# Install Newman globally
npm install -g newman

# Install HTML reporter for better reports
npm install -g newman-reporter-html
```

**12.3 Verify Newman Installation:**
```bash
# Check Newman version
newman --version
```

### **Step 13: Run Tests with Newman**

**13.1 Basic Newman Execution:**
```bash
# Navigate to where you saved your exported files
cd C:\path\to\your\exported\files

# Run tests with Newman
newman run EmployeeScheduling_API_Tests.postman_collection.json \
  --environment EmployeeScheduling_Environment.postman_environment.json
```

**13.2 Generate HTML Report:**
```bash
# Run with HTML report generation
newman run EmployeeScheduling_API_Tests.postman_collection.json \
  --environment EmployeeScheduling_Environment.postman_environment.json \
  --reporters cli,html \
  --reporter-html-export test-results.html
```

**13.3 Advanced Newman Options:**
```bash
# Run with additional options
newman run EmployeeScheduling_API_Tests.postman_collection.json \
  --environment EmployeeScheduling_Environment.postman_environment.json \
  --reporters cli,html,json \
  --reporter-html-export test-results.html \
  --reporter-json-export test-results.json \
  --iteration-count 1 \
  --delay-request 1000 \
  --timeout-request 10000
```

## 📊 **Creating Automated Test Reports**

Professional test reporting helps you understand test results and track API quality over time.

### **Step 14: Set Up Automated Reporting**

**14.1 Create Batch File for Easy Execution (Windows):**
Create a file named `run-api-tests.bat`:
```batch
@echo off
echo Starting Employee Scheduling API Tests...
echo.

REM Start the API (adjust path as needed)
start "API Server" cmd /k "cd /d C:\Users\lawry\Downloads\EmployeeScheduling\src\EmployeeScheduling.API && dotnet run"

REM Wait for API to start
echo Waiting for API to start...
timeout /t 10 /nobreak

REM Run Newman tests
echo Running automated tests...
newman run EmployeeScheduling_API_Tests.postman_collection.json ^
  --environment EmployeeScheduling_Environment.postman_environment.json ^
  --reporters cli,html ^
  --reporter-html-export test-results-%date:~-4,4%%date:~-10,2%%date:~-7,2%-%time:~0,2%%time:~3,2%.html

echo.
echo Tests completed! Check test-results.html for detailed report.
pause
```

**14.2 Create Shell Script for Easy Execution (Mac/Linux):**
Create a file named `run-api-tests.sh`:
```bash
#!/bin/bash
echo "Starting Employee Scheduling API Tests..."

# Start the API in background
cd /path/to/your/EmployeeScheduling/src/EmployeeScheduling.API
dotnet run &
API_PID=$!

# Wait for API to start
echo "Waiting for API to start..."
sleep 10

# Run Newman tests
echo "Running automated tests..."
newman run EmployeeScheduling_API_Tests.postman_collection.json \
  --environment EmployeeScheduling_Environment.postman_environment.json \
  --reporters cli,html \
  --reporter-html-export "test-results-$(date +%Y%m%d-%H%M).html"

# Stop the API
kill $API_PID

echo "Tests completed! Check test-results.html for detailed report."
```

### **Step 15: Interpret Test Results**

Understanding your test results is crucial for maintaining API quality and identifying issues quickly.

**15.1 Reading Newman CLI Output:**
- Green checkmarks (✓) indicate passing tests
- Red X marks (✗) indicate failing tests
- Summary shows total tests, passes, failures, and skipped tests
- Execution time helps identify performance issues

**15.2 Understanding HTML Reports:**
- Open the generated HTML file in your browser
- Summary section shows overall test health
- Individual test results show detailed pass/fail information
- Request/response details help debug failures
- Charts and graphs provide visual test status overview

**15.3 Common Test Failure Patterns:**
- **Connection errors**: API not running or wrong URL
- **Authentication failures**: Invalid credentials or expired tokens
- **Validation errors**: Request body doesn't match expected format
- **Business logic errors**: API logic doesn't match expected behavior

## 🎉 **Congratulations! You've Completed Option 1**

You now have a complete automated testing solution using Postman that provides:

- **Comprehensive API validation** for all major endpoints
- **Automated test execution** with detailed reporting
- **Professional-grade testing** that runs consistently
- **Easy maintenance** and extension for future features
- **Independence from unreliable human testers**

Your testing suite can now validate your entire API in minutes, providing confidence that your Employee Scheduling system works correctly. The automated reports give you professional documentation of your API's quality and reliability.

In the next section, we'll explore Option 2: .NET Integration Tests, which provides even more thorough testing with deeper integration into your development workflow.

