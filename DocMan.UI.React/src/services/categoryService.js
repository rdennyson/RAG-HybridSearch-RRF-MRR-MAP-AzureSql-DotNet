import apiClient from './apiClient'

export const categoryService = {
  // Get all categories
  getCategories: async () => {
    try {
      const response = await apiClient.get('/api/categories')
      return response.data
    } catch (error) {
      console.error('Error fetching categories:', error)
      throw error
    }
  },

  // Get category by ID
  getCategoryById: async (categoryId) => {
    try {
      const response = await apiClient.get(`/api/categories/${categoryId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching category:', error)
      throw error
    }
  },

  // Create category
  createCategory: async (name, description = '', color = '#3182CE') => {
    try {
      const response = await apiClient.post('/api/categories', {
        name,
        description,
        color,
      })
      return response.data
    } catch (error) {
      console.error('Error creating category:', error)
      throw error
    }
  },

  // Update category
  updateCategory: async (categoryId, name, description = '', color = '#3182CE') => {
    try {
      const response = await apiClient.put(`/api/categories/${categoryId}`, {
        name,
        description,
        color,
      })
      return response.data
    } catch (error) {
      console.error('Error updating category:', error)
      throw error
    }
  },

  // Delete category
  deleteCategory: async (categoryId) => {
    try {
      const response = await apiClient.delete(`/api/categories/${categoryId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting category:', error)
      throw error
    }
  },
}

