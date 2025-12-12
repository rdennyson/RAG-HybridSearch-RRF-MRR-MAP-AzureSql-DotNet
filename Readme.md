# DocMan - Document Management & RAG System

## 🎯 Project Overview

**DocMan** is an enterprise-grade **Retrieval-Augmented Generation (RAG)** system built with **.NET 9** and **React 18** that enables intelligent document management and semantic search with advanced AI capabilities.

The system allows users to upload documents (PDF, DOCX, TXT, MD), automatically processes them into semantic chunks with embeddings, and provides intelligent search using hybrid retrieval techniques combined with LLM-powered answers.

---

## 🏗️ Architecture

### **Backend Stack** (.NET 9)
- **DocMan.API** - ASP.NET Core Minimal APIs with JWT authentication
- **DocMan.Core** - Business logic, CQRS with MediatR, RAG services
- **DocMan.Data** - Entity Framework Core 9.0 with Azure SQL Server
- **DocMan.Model** - Entity models and DTOs

### **Frontend Stack** (React 18)
- **DocMan.UI.React** - React with Vite, Chakra UI, responsive design
- Dark/Light theme support
- Mobile-friendly interface

### **AI/ML Services**
- **Azure OpenAI** - Embeddings (text-embedding-3-small) & Chat (GPT-4o)
- **Semantic Kernel** - LLM orchestration framework
- **Tiktoken** - Token counting for efficient context management

---

## ✨ Key Features

### 📄 Document Management
- Upload documents (PDF, DOCX, TXT, Markdown)
- Automatic content extraction and chunking
- Vector embeddings generation (1536 dimensions)
- Document categorization and organization

### 🔍 Hybrid Search (5 Modes)
1. **Dense Only** - Pure vector similarity search
2. **Sparse Only** - BM25 keyword-based search
3. **Hybrid** - Dense + Sparse with RRF fusion
4. **Hybrid + HyDE** - Hypothetical document generation
5. **Full Pipeline** - HyDE + Cross-encoder reranking

### 🤖 Advanced RAG Techniques
- **BM25** - Probabilistic ranking for keyword search
- **RRF** - Reciprocal Rank Fusion for result combination
- **HyDE** - Hypothetical Document Embeddings via LLM
- **Cross-Encoder Reranking** - Semantic similarity-based reranking
- **Token Management** - Efficient context window handling

### 💬 Chat Interface
- Real-time search with metrics
- LLM-generated answers with source attribution
- Evaluation mode for comparing retrieval strategies
- Execution time tracking

### 🔐 Security
- JWT authentication with role-based access
- User-scoped document access
- Secure API endpoints

---

## 📊 Database Schema

**Core Entities:**
- **Users** - Authentication & authorization
- **Documents** - Document metadata
- **DocumentChunks** - Semantic chunks with vector embeddings
- **Categories** - Document organization

**Vector Search:**
- SQL Server vector columns (float[1536])
- Cosine similarity distance function
- Efficient indexing for fast retrieval

---

## 🚀 Technology Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | .NET 9, ASP.NET Core, EF Core 9.0 |
| **Database** | Azure SQL Server with Vector Search |
| **Frontend** | React 18, Vite, Chakra UI |
| **AI/ML** | Azure OpenAI, Semantic Kernel |
| **Search** | Lucene.Net (BM25), Vector DB |
| **Architecture** | CQRS, Repository Pattern, Unit of Work |

---

## 🎮 Getting Started

### Backend
```bash
cd DocMan.API
dotnet run
# API runs on http://localhost:5021
```

### Frontend
```bash
cd DocMan.UI.React
npm install
npm run dev
# UI runs on http://localhost:5174
```

### Demo Credentials
- **Username:** john_doe
- **Password:** Password123!

---

## 📈 Workflow

1. **User Login** → JWT token issued
2. **Document Upload** → Content extracted, chunked, embedded
3. **BM25 Indexing** → Sparse index built automatically
4. **Search Query** → Hybrid retrieval executed
5. **LLM Generation** → Answer synthesized from context
6. **Response** → Answer + sources + metrics returned

---

## 🔧 Configuration

**appsettings.json:**
```json
{
  "AzureOpenAI": {
    "Embedding": { "Endpoint", "Deployment", "ModelId", "ApiKey" },
    "ChatCompletion": { "Endpoint", "Deployment", "ModelId", "ApiKey" }
  },
  "AppSettings": {
    "MaxInputTokens": 16385,
    "MaxOutputTokens": 800,
    "MaxRelevantChunks": 5
  }
}
```

---

## 📝 Project Status

✅ **Complete Implementation:**
- Full RAG pipeline with hybrid search
- Advanced retrieval techniques (BM25, RRF, HyDE, Cross-Encoder)
- React UI with search mode selection
- Token-efficient context management
- Evaluation metrics system

🚀 **Production Ready** - All core features implemented and tested

