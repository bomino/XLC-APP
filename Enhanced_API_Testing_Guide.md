# 📋 **Enhanced Employee Scheduling API - Complete Testing Guide**

## 🎯 **Overview**

This comprehensive testing guide covers the complete Enhanced Employee Scheduling API with both Employee Management and Schedule Management functionality. The API now includes 74+ endpoints across 7 controllers with full CRUD operations, role-based authorization, and advanced scheduling features.

## 🔧 **Prerequisites**

### **Required Software:**
- .NET 8.0 SDK
- SQL Server Express or SQL Server
- Visual Studio 2022 or VS Code
- Postman or Swagger UI for API testing

### **Project Setup:**
1. **Extract the Enhanced API project** to your development environment
2. **Open in Visual Studio or VS Code**
3. **Restore NuGet packages:** `dotnet restore`
4. **Update connection string** in `appsettings.json`

## 🗄️ **Database Setup**

### **Step 1: Create Database Migration**
```bash
# Navigate to project directory
cd EmployeeScheduling.API.Enhanced

# Create migration for Schedule Management
dotnet ef migrations add AddScheduleManagement

# Update database
dotnet ef database update
```

### **Step 2: Verify Database Schema**
The migration will create these new tables:
- **Schedules** - Schedule management
- **Shifts** - Individual work shifts
- **Assignments** - Employee shift assignments
- **Availabilities** - Employee availability by day
- **Locations** - Work locations
- **Qualifications** - Employee qualifications
- **EmployeeQualifications** - Employee-qualification relationships
- **ShiftQualifications** - Shift-qualification requirements

## 🚀 **Starting the API**

### **Step 1: Build and Run**
```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

### **Step 2: Access Swagger UI**
- **URL:** `https://localhost:5001/swagger` or `http://localhost:5000/swagger`
- **Features:** Interactive API documentation with JWT authentication

## 🔐 **Authentication Setup**

### **Step 1: Login and Get JWT Token**

**Endpoint:** `POST /api/Auth/login`

**Test Credentials:**
```json
{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "userId": 1,
      "firstName": "Admin",
      "lastName": "User",
      "email": "admin@example.com",
      "role": "Admin"
    },
    "expiresAt": "2025-06-07T12:00:00Z"
  }
}
```

### **Step 2: Authorize Swagger**
1. **Click the "Authorize" button** in Swagger UI
2. **Enter:** `Bearer [your-jwt-token]`
3. **Click "Authorize"** and **"Close"**

## 📊 **Testing Phases**



### **Phase 1: Employee Management Testing (Existing Functionality)**

#### **Test 1.1: User Management**

**Create User:**
```json
POST /api/Users
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "password": "JohnDoe123!",
  "role": "Employee"
}
```

**Get All Users:**
```
GET /api/Users
```

#### **Test 1.2: Employee Management**

**Create Employee:**
```json
POST /api/Employees
{
  "userId": 2,
  "employeeNumber": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "phoneNumber": "555-0123",
  "address": "123 Main St, City, State 12345",
  "dateOfBirth": "1990-05-15",
  "hireDate": "2024-01-15",
  "department": "Engineering",
  "position": "Software Developer",
  "hourlyRate": 35.00,
  "employmentType": "Full-Time",
  "maxHoursPerWeek": 40,
  "minHoursPerWeek": 32,
  "notes": "Experienced developer"
}
```

**Get All Employees:**
```
GET /api/Employees?page=1&pageSize=10
```

**Update Employee:**
```json
PUT /api/Employees/1
{
  "phoneNumber": "555-9999",
  "position": "Senior Software Developer",
  "hourlyRate": 45.00
}
```

### **Phase 2: Schedule Management Testing (New Functionality)**

#### **Test 2.1: Schedule Management**

**Create Schedule:**
```json
POST /api/Schedules
{
  "name": "Week of June 10-16, 2025",
  "description": "Regular weekly schedule",
  "startDate": "2025-06-10",
  "endDate": "2025-06-16",
  "isPublished": false
}
```

**Get All Schedules:**
```
GET /api/Schedules?page=1&pageSize=10
```

**Publish Schedule:**
```
POST /api/Schedules/1/publish
```

**Get Current Schedule:**
```
GET /api/Schedules/current
```

#### **Test 2.2: Shift Management**

**Create Shift:**
```json
POST /api/Shifts
{
  "scheduleId": 1,
  "title": "Morning Shift - Engineering",
  "description": "Regular morning shift for engineering team",
  "startTime": "2025-06-10T08:00:00",
  "endTime": "2025-06-10T16:00:00",
  "requiredEmployees": 3,
  "locationId": 1,
  "department": "Engineering",
  "position": "Software Developer"
}
```

**Get Shifts by Schedule:**
```
GET /api/Shifts/schedule/1
```

**Get Today's Shifts:**
```
GET /api/Shifts/today
```

**Get Available Employees for Shift:**
```
GET /api/Shifts/1/available-employees
```

#### **Test 2.3: Employee Availability**

**Create Employee Availability:**
```json
POST /api/Availability
{
  "employeeId": 1,
  "dayOfWeek": 1,
  "startTime": "08:00:00",
  "endTime": "17:00:00",
  "availabilityType": "Available",
  "notes": "Regular Monday availability"
}
```

**Bulk Create Availability (Full Week):**
```json
POST /api/Availability/bulk
{
  "employeeId": 1,
  "availabilityRequests": [
    {
      "employeeId": 1,
      "dayOfWeek": 1,
      "startTime": "08:00:00",
      "endTime": "17:00:00",
      "availabilityType": "Available"
    },
    {
      "employeeId": 1,
      "dayOfWeek": 2,
      "startTime": "08:00:00",
      "endTime": "17:00:00",
      "availabilityType": "Available"
    },
    {
      "employeeId": 1,
      "dayOfWeek": 3,
      "startTime": "08:00:00",
      "endTime": "17:00:00",
      "availabilityType": "Preferred"
    }
  ]
}
```

**Get Employee Availability:**
```
GET /api/Availability/employee/1
```

#### **Test 2.4: Shift Assignments**

**Create Assignment:**
```json
POST /api/Assignments
{
  "employeeId": 1,
  "shiftId": 1,
  "notes": "Regular assignment"
}
```

**Bulk Create Assignments:**
```json
POST /api/Assignments/bulk
{
  "assignments": [
    {
      "employeeId": 1,
      "shiftId": 1,
      "notes": "Morning shift lead"
    },
    {
      "employeeId": 2,
      "shiftId": 1,
      "notes": "Morning shift support"
    }
  ]
}
```

**Employee Confirms Assignment:**
```
POST /api/Assignments/1/confirm
```

**Employee Checks In:**
```json
POST /api/Assignments/1/checkin
{
  "checkInTime": "2025-06-10T08:00:00"
}
```

**Employee Checks Out:**
```json
POST /api/Assignments/1/checkout
{
  "checkOutTime": "2025-06-10T16:00:00"
}
```

**Get My Assignments:**
```
GET /api/Assignments/my-assignments
```

**Get Today's Assignments:**
```
GET /api/Assignments/my-assignments/today
```

### **Phase 3: Advanced Features Testing**

#### **Test 3.1: Search and Filtering**

**Search Employees:**
```
GET /api/Employees?search=john&department=Engineering&page=1&pageSize=10
```

**Filter Shifts by Date Range:**
```
GET /api/Shifts/date-range?startDate=2025-06-10&endDate=2025-06-16
```

**Search Assignments:**
```
GET /api/Assignments?employeeId=1&status=Assigned&startDate=2025-06-10
```

#### **Test 3.2: Conflict Detection**

**Check Shift Conflicts:**
```
GET /api/Shifts/conflicts?employeeId=1&startTime=2025-06-10T08:00:00&endTime=2025-06-10T16:00:00
```

**Get Understaffed Shifts:**
```
GET /api/Shifts/understaffed?startDate=2025-06-10&endDate=2025-06-16
```

#### **Test 3.3: Role-Based Access Testing**

**Test Employee Role Restrictions:**
1. **Login as Employee** (create employee user first)
2. **Try to access admin endpoints** (should get 403 Forbidden)
3. **Access own data only** (should work)

**Test Manager Role:**
1. **Create Manager user** with role "Manager"
2. **Test schedule and shift management** (should work)
3. **Test employee management** (should work)

### **Phase 4: Error Handling Testing**

#### **Test 4.1: Validation Errors**

**Invalid Employee Creation:**
```json
POST /api/Employees
{
  "userId": 999,
  "employeeNumber": "",
  "firstName": "",
  "email": "invalid-email"
}
```
*Expected: 400 Bad Request with validation errors*

**Duplicate Employee Number:**
```json
POST /api/Employees
{
  "employeeNumber": "EMP001"
}
```
*Expected: 400 Bad Request - duplicate employee number*

#### **Test 4.2: Authorization Errors**

**Access Without Token:**
```
GET /api/Employees
```
*Expected: 401 Unauthorized*

**Access Wrong Role:**
```
DELETE /api/Schedules/1
```
*(as Employee role)*
*Expected: 403 Forbidden*

#### **Test 4.3: Business Logic Errors**

**Conflicting Assignment:**
```json
POST /api/Assignments
{
  "employeeId": 1,
  "shiftId": 2
}
```
*(when employee already has overlapping shift)*
*Expected: 400 Bad Request - conflict detected*

**Check Out Without Check In:**
```
POST /api/Assignments/1/checkout
```
*(without checking in first)*
*Expected: 400 Bad Request - must check in first*


