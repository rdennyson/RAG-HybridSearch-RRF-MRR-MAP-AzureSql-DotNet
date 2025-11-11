import apiClient from './apiClient'

// Hybrid search modes
export const HybridSearchMode = {
  DenseOnly: 0,
  SparseOnly: 1,
  Hybrid: 2,
  HybridWithHyDE: 3,
  HybridWithHyDEAndReranking: 4,
}

export const searchService = {
  // Search documents with optional metrics and hybrid search mode
  search: async (query, topK = 10, includeMetrics = false, searchMode = HybridSearchMode.Hybrid, useReranking = false) => {
    try {
      const response = await apiClient.post('/api/search/query', {
        query,
        topK,
        includeMetrics,
        searchMode,
        useReranking,
      })
      return response.data
    } catch (error) {
      console.error('Error searching documents:', error)
      throw error
    }
  },

  // Search with metrics (for evaluation mode)
  searchWithMetrics: async (query, topK = 10, searchMode = HybridSearchMode.Hybrid, useReranking = false) => {
    return searchService.search(query, topK, true, searchMode, useReranking)
  },

  // Search without metrics (for normal mode)
  searchWithoutMetrics: async (query, topK = 10, searchMode = HybridSearchMode.Hybrid, useReranking = false) => {
    return searchService.search(query, topK, false, searchMode, useReranking)
  },

  // Dual search for evaluation mode (returns both with and without metrics)
  dualSearch: async (query, topK = 10, searchMode = HybridSearchMode.Hybrid, useReranking = false) => {
    try {
      const [withMetrics, withoutMetrics] = await Promise.all([
        searchService.searchWithMetrics(query, topK, searchMode, useReranking),
        searchService.searchWithoutMetrics(query, topK, searchMode, useReranking),
      ])
      return {
        withMetrics,
        withoutMetrics,
      }
    } catch (error) {
      console.error('Error in dual search:', error)
      throw error
    }
  },
}

