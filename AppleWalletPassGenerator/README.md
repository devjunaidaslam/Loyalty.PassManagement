# Apple Wallet Pass Generator

A .NET 8 ASP.NET Core API for generating, managing, and updating Apple Wallet (Passbook) loyalty cards with push notification support.

## Overview

This application enables businesses to create digital loyalty cards compatible with Apple Wallet. It implements Apple's Web Service API standards for pass distribution, device registration, and real-time updates via push notifications.

## Features

- **Pass Generation**: Create .pkpass files with customer loyalty card data
- **Points Management**: Update loyalty points with automatic push notifications
- **Device Registration**: Register/unregister devices for pass updates
- **Web Service API**: Full compliance with Apple's Wallet Web Service standards
- **Push Notifications**: Send real-time updates to registered devices
- **Data Persistence**: SQL Server database for pass and customer data

## Tech Stack

- **.NET 8** with ASP.NET Core
- **Entity Framework Core** for data access
- **SQL Server** as the database
- **Apple Wallet Web Service API** integration

## Prerequisites

- .NET 8 SDK
- SQL Server (local or remote)
- Apple Developer Account (for pass type identifier and certificates)

## Configuration

Update `appsettings.json` with your settings:

```json
"PassSettings": {
  "PassTypeIdentifier": "pass.com.yourcompany.app",
  "TeamIdentifier": "YOUR_TEAM_IDENTIFIER",
  "OrganizationName": "Your Organization",
  "LogoText": "Loyalty Program Name",
  "WebServiceURL": "https://yourdomain.com/Pass/",
  "AuthenticationToken": "YOUR_BASE64_AUTH_TOKEN",
  "BackgroundColor": "#FFFFFF",
  "LabelColor": "#000000",
  "ForegroundColor": "#000000"
}
```

Update the database connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=AppleWalletPassGenerator;User Id=sa;Password=123;"
}
```

## API Endpoints

### Pass Management
- `POST /pass/generate` - Create a new loyalty pass
- `POST /pass/update-points` - Update customer points and send push notification
- `GET /pass/GetAllCustomer` - Retrieve all customers

### Apple Wallet Web Service (RFC 6962)
- `GET /pass/v1/passes/{passTypeIdentifier}/{serialNumber}` - Fetch pass
- `POST /pass/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}` - Register device
- `GET /pass/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}` - Get device registrations
- `DELETE /pass/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}` - Unregister device
- `GET /pass/passes/{serialNumber}/log` - Get pass update log

## Usage

### Generate a Pass

```bash
POST /pass/generate
Content-Type: application/json

{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "points": 100,
  "barcodeMessage": "123456789",
  "qrCodeData": "LOYALTY_ABC123"
}
```

### Update Points

```bash
POST /pass/update-points
Content-Type: application/json

{
  "serialNumber": "550e8400-e29b-41d4-a716-446655440000",
  "points": 150
}
```

## Getting Started

1. Clone the repository
2. Configure `appsettings.json` with your settings
3. Run migrations: `dotnet ef database update`
4. Start the application: `dotnet run`
5. Access Swagger UI at `https://localhost:5001/swagger`

## Project Structure

```
AppleWalletPassGenerator/
??? Controllers/          # API endpoints
??? Services/            # Business logic
??? IServices/           # Service interfaces
??? Models/              # Data models
??? appsettings.json     # Configuration
```

## License

Proprietary - All rights reserved
