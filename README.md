# AqvaTech Question

## Versions
 - This project was made with .NET 8.0.

## Dependencies
- Elastic.Clients.Elasticsearch => 8.14.8
- RabbitMQ.Client => 6.8.1
- HtmlAgilityPack => 1.11.62
- Microsoft.Extensions.Configuration.Abstractions => 8.0.0
- Newtonsoft.Json => 13.0.3

## Project Info
1. **Author Search Project Architecture**:
      - The architecture is divided into two main parts: Author Search (Blazor) and the Backend Business Logic system.

## Frontend: Author Search Project Architecture
1. **Web Client**: The user interface for the search functionality.
2. **Search Service**:
   - Implemented in .NET
   - Provides a Search API for the web client
4. **Presentation Layer**:
    - **Aqva_API**: Handles the search interface
    - **SearchController**: Manages search requests
    - **ColumnistRepository**: Stores and retrieves columnist data
    - **IndexerBackgroundService**: Background service for indexing
    - **RabbitMqConsumer**: Consumes data from RabbitMQ

## Backend: Job Processing Architecture
1. **Application Layer**:
    - **App WebCrawler**: Main application for web crawling
    - **CrawlerService**: Handles the crawling process

2. **Infrastructure Layer**:
    - **Infra WebCrawler**: Infrastructure for web crawling
    - **RabbitMq**: Message queue for communication
    - **Infra ColumnistRepository**: Stores crawled columnist data

3. **Repository**:
     - **Repo ColumnistRepository**: Manages columnist data storage

5. **Persistence**:
     - **ColumnistRepository**: Persistent storage for columnist data

## Data Flow
1. The Web Client interacts with the .NET Search API.
2. The Search API communicates with the Presentation layer's Aqva_API.
3. The SearchController accesses the ColumnistRepository to retrieve search results.

## Key Components
- **Web Crawler**: Responsible for gathering author and content data from web sources.
- **RabbitMQ**: Facilitates asynchronous communication between components.
- **Indexer**: Processes and indexes the crawled data for efficient searching.
- **Search API**: Provides search functionality to the frontend.
- **Repositories**: Store and manage columnist and content data.

## Technologies Used
- .NET for the backend API
- RabbitMQ for message queuing
- Web crawling technologies 
- Database for data persistence

## Setup
1. **Prerequisites**:
   - Install .NET SDK 8.0
   - Install RabbitMQ server
   - Setup a ElasticSearch

2. **Backend Setup**:
   - You need the fill that appsettings.json
   - **DO NOT CHANGE "DefaultIndex" NAME.**

  ```json
   "RabbitMq": {
    "HostName": "",
    "UserName": "",
    "Password": "",
    "Port": 5672
  },
  "Elasticsearch": {
    "Uri": "",
    "CertificateFingerprint": "",
    "UserName": "",
    "Password": "",
    "DefaultIndex": "columnists"
  },
  "Crawler": {
    "BaseUrl": "https://www.sozcu.com.tr/yazarlar",
    "Time": 30
  }
 ```

