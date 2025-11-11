import apiClient from './apiClient'

export const userService = {
  // Get all users
  getUsers: async () => {
    try {
      const response = await apiClient.get('/api/users')
      return response.data
    } catch (error) {
      console.error('Error fetching users:', error)
      throw error
    }
  },

  // Get user by ID
  getUserById: async (userId) => {
    try {
      const response = await apiClient.get(`/api/users/${userId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching user:', error)
      throw error
    }
  },

  // Create user
  createUser: async (username, email, password) => {
    try {
      const response = await apiClient.post('/api/users', {
        username,
        email,
        password,
      })
      return response.data
    } catch (error) {
      console.error('Error creating user:', error)
      throw error
    }
  },

  // Update user
  updateUser: async (userId, email) => {
    try {
      const response = await apiClient.put(`/api/users/${userId}`, {
        email,
      })
      return response.data
    } catch (error) {
      console.error('Error updating user:', error)
      throw error
    }
  },

  // Delete user
  deleteUser: async (userId) => {
    try {
      const response = await apiClient.delete(`/api/users/${userId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting user:', error)
      throw error
    }
  },
}

