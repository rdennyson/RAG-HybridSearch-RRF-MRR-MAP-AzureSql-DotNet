import { useState, useEffect, useRef } from 'react'
import {
  Box,
  VStack,
  HStack,
  Input,
  Button,
  Flex,
  Text,
  Spinner,
  Badge,
  useColorMode,
  Divider,
  FormControl,
  FormLabel,
  NumberInput,
  NumberInputField,
  Switch,
  Tooltip,
  Select,
} from '@chakra-ui/react'
import { searchService, HybridSearchMode } from '../services/searchService'

export default function ChatInterface() {
  const { colorMode } = useColorMode()
  const [query, setQuery] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [evaluationMode, setEvaluationMode] = useState(false)
  const [topK, setTopK] = useState(10)
  const [searchMode, setSearchMode] = useState(HybridSearchMode.Hybrid)
  const [useReranking, setUseReranking] = useState(false)
  const [results, setResults] = useState(null)
  const [dualResults, setDualResults] = useState(null)
  const messagesEndRef = useRef(null)

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [results, dualResults])

  const handleSearch = async (e) => {
    e.preventDefault()
    if (!query.trim()) return

    setIsLoading(true)
    try {
      if (evaluationMode) {
        const dual = await searchService.dualSearch(query, topK, searchMode, useReranking)
        setDualResults(dual)
        setResults(null)
      } else {
        const result = await searchService.searchWithoutMetrics(query, topK, searchMode, useReranking)
        setResults(result)
        setDualResults(null)
      }
    } catch (error) {
      console.error('Search error:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const renderMetrics = (metrics) => {
    if (!metrics) return null

    return (
      <VStack spacing={2} align="stretch" fontSize="sm">
        <HStack justify="space-between">
          <Text fontWeight="600">MRR:</Text>
          <Badge colorScheme="blue">{(metrics.meanReciprocalRank * 100).toFixed(2)}%</Badge>
        </HStack>
        <HStack justify="space-between">
          <Text fontWeight="600">Precision@K:</Text>
          <Badge colorScheme="green">{(metrics.precisionAtK * 100).toFixed(2)}%</Badge>
        </HStack>
        <HStack justify="space-between">
          <Text fontWeight="600">Recall@K:</Text>
          <Badge colorScheme="purple">{(metrics.recallAtK * 100).toFixed(2)}%</Badge>
        </HStack>
        <HStack justify="space-between">
          <Text fontWeight="600">NDCG@K:</Text>
          <Badge colorScheme="orange">{(metrics.ndcgAtK * 100).toFixed(2)}%</Badge>
        </HStack>
        <HStack justify="space-between">
          <Text fontWeight="600">Avg Precision:</Text>
          <Badge colorScheme="cyan">{(metrics.averagePrecision * 100).toFixed(2)}%</Badge>
        </HStack>
      </VStack>
    )
  }

  const renderResults = (data, showMetrics = false) => {
    if (!data) return null

    return (
      <VStack spacing={4} align="stretch">
        {/* LLM Answer Section */}
        <Box p={4} bg={colorMode === 'light' ? 'blue.50' : 'blue.900'} borderRadius="lg" borderLeft="4px" borderColor="blue.500">
          <Text fontWeight="700" mb={2} color="blue.600">
            💡 Answer
          </Text>
          <Text fontSize="md" lineHeight="1.6" color={colorMode === 'light' ? 'gray.800' : 'gray.100'}>
            {data.answer || 'No answer generated'}
          </Text>
          <Text fontSize="xs" mt={3} color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
            Generated in {data.executionTimeMs.toFixed(2)}ms
          </Text>
        </Box>

        {/* Retrieved Sources Section */}
        <Box>
          <Text fontWeight="600" mb={3} fontSize="sm">
            📚 Retrieved Sources ({data.retrievedChunks?.length || 0})
          </Text>

          {data.retrievedChunks && data.retrievedChunks.length > 0 ? (
            <VStack spacing={2} align="stretch">
              {data.retrievedChunks.map((chunk, idx) => (
                <Box
                  key={chunk.chunkId}
                  p={3}
                  bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
                  borderRadius="md"
                  borderLeft="3px"
                  borderColor="green.500"
                >
                  <HStack justify="space-between" mb={1}>
                    <Text fontWeight="600" fontSize="sm">
                      #{idx + 1} - {chunk.documentName}
                    </Text>
                  </HStack>
                  <Text fontSize="xs" color={colorMode === 'light' ? 'gray.700' : 'gray.300'} mb={2}>
                    {chunk.content.substring(0, 150)}...
                  </Text>
                  <Badge variant="subtle" fontSize="xs">Page {chunk.pageNumber}</Badge>
                </Box>
              ))}
            </VStack>
          ) : (
            <Text fontSize="sm" color={colorMode === 'light' ? 'gray.500' : 'gray.400'}>
              No sources retrieved
            </Text>
          )}
        </Box>

        {/* Metrics Section */}
        {showMetrics && data.metrics && (
          <Box p={4} bg={colorMode === 'light' ? 'yellow.50' : 'yellow.900'} borderRadius="lg" borderLeft="4px" borderColor="yellow.500">
            <Text fontWeight="600" mb={3}>
              📊 Retrieval Metrics
            </Text>
            {renderMetrics(data.metrics)}
          </Box>
        )}
      </VStack>
    )
  }

  return (
    <VStack h="full" spacing={0}>
      {/* Results Area */}
      <Box flex="1" w="full" overflowY="auto" p={4}>
        {evaluationMode && dualResults ? (
          <HStack spacing={4} align="start" h="full">
            {/* With Metrics */}
            <Box flex="1" p={4} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRadius="lg" boxShadow="sm" overflowY="auto">
              <Text fontWeight="700" mb={4} color="blue.500">
                📊 With Metrics
              </Text>
              {renderResults(dualResults.withMetrics, true)}
            </Box>

            <Divider orientation="vertical" />

            {/* Without Metrics */}
            <Box flex="1" p={4} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRadius="lg" boxShadow="sm" overflowY="auto">
              <Text fontWeight="700" mb={4} color="green.500">
                ✓ Without Metrics
              </Text>
              {renderResults(dualResults.withoutMetrics, false)}
            </Box>
          </HStack>
        ) : results ? (
          <Box p={4} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRadius="lg" boxShadow="sm">
            {renderResults(results, false)}
          </Box>
        ) : (
          <Flex justify="center" align="center" h="full">
            <Text color={colorMode === 'light' ? 'gray.500' : 'gray.400'}>
              Enter a query to search documents
            </Text>
          </Flex>
        )}
        <div ref={messagesEndRef} />
      </Box>

      {/* Search Input */}
      <Box w="full" p={4} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderTop="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}>
        <form onSubmit={handleSearch}>
          <VStack spacing={3} align="stretch">
            <HStack spacing={2}>
              <Input
                placeholder="Ask a question about your documents..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                disabled={isLoading}
                size="lg"
                bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
              />
              <NumberInput
                value={topK}
                onChange={(val) => setTopK(parseInt(val) || 10)}
                min={1}
                max={100}
                w="100px"
              >
                <NumberInputField placeholder="TopK" size="lg" bg={colorMode === 'light' ? 'gray.50' : 'gray.700'} />
              </NumberInput>
              <Button colorScheme="blue" type="submit" isLoading={isLoading} size="lg">
                Search
              </Button>
            </HStack>
            <HStack justify="flex-end" spacing={4} flexWrap="wrap">
              <HStack spacing={2}>
                <Text fontSize="sm" fontWeight="500">Search Mode</Text>
                <Tooltip label="Select hybrid search strategy">
                  <Select
                    value={searchMode}
                    onChange={(e) => setSearchMode(parseInt(e.target.value))}
                    w="180px"
                    size="sm"
                    bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
                  >
                    <option value={HybridSearchMode.DenseOnly}>Dense Only</option>
                    <option value={HybridSearchMode.SparseOnly}>Sparse Only (BM25)</option>
                    <option value={HybridSearchMode.Hybrid}>Hybrid (Dense + Sparse)</option>
                    <option value={HybridSearchMode.HybridWithHyDE}>Hybrid + HyDE</option>
                    <option value={HybridSearchMode.HybridWithHyDEAndReranking}>Full Pipeline</option>
                  </Select>
                </Tooltip>
              </HStack>
              <HStack spacing={2}>
                <Text fontSize="sm" fontWeight="500">Reranking</Text>
                <Tooltip label="Enable cross-encoder reranking for better results">
                  <Switch isChecked={useReranking} onChange={(e) => setUseReranking(e.target.checked)} />
                </Tooltip>
              </HStack>
              <HStack spacing={2}>
                <Text fontSize="sm" fontWeight="500">Evaluation Mode</Text>
                <Tooltip label="Compare results with and without metrics">
                  <Switch isChecked={evaluationMode} onChange={(e) => setEvaluationMode(e.target.checked)} />
                </Tooltip>
              </HStack>
            </HStack>
          </VStack>
        </form>
      </Box>
    </VStack>
  )
}

