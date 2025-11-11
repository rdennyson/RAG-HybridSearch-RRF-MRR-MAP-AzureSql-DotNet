import apiClient from './apiClient'

export const authService = {
  login: async (username, password) => {
    try {
      const response = await apiClient.post('/api/auth/login', {
        username,
        password,
      })
      const { token, ...user } = response.data
      localStorage.setItem('auth_token', token)
      localStorage.setItem('user', JSON.stringify(user))
      return response.data
    } catch (error) {
      console.error('Login error:', error)
      throw error
    }
  },

  register: async (username, email, password, fullName) => {
    try {
      const response = await apiClient.post('/api/auth/register', {
        username,
        email,
        password,
        fullName,
      })
      const { token, ...user } = response.data
      localStorage.setItem('auth_token', token)
      localStorage.setItem('user', JSON.stringify(user))
      return response.data
    } catch (error) {
      console.error('Register error:', error)
      throw error
    }
  },

  logout: () => {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user')
  },

  getCurrentUser: () => {
    const user = localStorage.getItem('user')
    return user ? JSON.parse(user) : null
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('auth_token')
  },
}

