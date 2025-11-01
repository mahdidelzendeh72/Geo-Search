# Geo-Search

A Redis Stack-based geospatial search API for managing and querying gas stations. This project demonstrates storing, retrieving, and searching geospatial data using **Redis GEO commands** in ASP.NET Core.

---

## 🌟 Features

- Store gas stations with geospatial coordinates in Redis
- Search gas stations within a bounding box
- Find the nearest gas station to a coordinate
- Seed sample gas stations automatically via a background service
- Use aversine distance formula.
<img width="470" height="300" alt="Gemini_Generated_Image_sravzksravzksrav" src="https://github.com/user-attachments/assets/d6417aa8-ab52-4f7e-9938-6cad7720ae62" />

---

## Technology Stack

- ASP.NET Core 7
- C# 11
- StackExchange.Redis (v2.7+) for Redis Stack integration
- Redis Stack (with GEO support)

---

## Getting Started

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Redis Stack](https://redis.io/docs/stack/) running locally or in Docker
- (Optional) VS Code with REST Client extension for testing

### Clone the Repository

```bash
git clone https://github.com/mahdidelzendeh72/Geo-Search.git
cd Geo-Search
```
Configure Redis Connection
```bash
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

Run Redis On Docket
```bash
docker run -d --name redis -p 6379:6379 redis:<version>
```

Run the Application
```bash
dotnet run
```
API Endpoints
| Endpoint                                      | Method | Description                                  |
| --------------------------------------------- | ------ | -------------------------------------------- |
| `/api/gasstationgeo/set`                      | POST   | Add or update gas station geo data           |
| `/api/gasstationgeo/search-bounding-box`      | POST   | Search gas stations within a bounding box    |
| `/api/gasstationgeo/find-nearest-gas-station` | POST   | Find the nearest gas station to a coordinate |
