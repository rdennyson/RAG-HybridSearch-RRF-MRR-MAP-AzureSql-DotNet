import apiClient, { fileUploadClient } from './apiClient'

export const documentService = {
  // Get all documents
  getDocuments: async () => {
    try {
      const response = await apiClient.get('/api/documents')
      return response.data
    } catch (error) {
      console.error('Error fetching documents:', error)
      throw error
    }
  },

  // Get document by ID
  getDocumentById: async (documentId) => {
    try {
      const response = await apiClient.get(`/api/documents/${documentId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching document:', error)
      throw error
    }
  },

  // Upload document
  uploadDocument: async (file, categoryId = null) => {
    try {
      const formData = new FormData()
      formData.append('file', file)
      if (categoryId) {
        formData.append('categoryId', categoryId)
      }

      const response = await fileUploadClient.post('/api/documents/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      })
      return response.data
    } catch (error) {
      console.error('Error uploading document:', error)
      throw error
    }
  },

  // Delete document
  deleteDocument: async (documentId) => {
    try {
      const response = await apiClient.delete(`/api/documents/${documentId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting document:', error)
      throw error
    }
  },
}

