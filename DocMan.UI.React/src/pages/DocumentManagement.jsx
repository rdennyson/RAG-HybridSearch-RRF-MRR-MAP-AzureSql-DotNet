import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Flex,
  VStack,
  HStack,
  Heading,
  Button,
  useColorMode,
  Table,
  Thead,
  Tbody,
  Tr,
  Th,
  Td,
  TableContainer,
  Badge,
  IconButton,
  useDisclosure,
  Spinner,
  Alert,
  AlertIcon,
  Text,
} from '@chakra-ui/react'
import { documentService } from '../services/documentService'
import { authService } from '../services/authService'
import Sidebar from '../components/Sidebar'
import DocumentUploadModal from '../components/DocumentUploadModal'

export default function DocumentManagement() {
  const navigate = useNavigate()
  const { colorMode } = useColorMode()
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [documents, setDocuments] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadDocuments()
  }, [])

  const loadDocuments = async () => {
    try {
      setIsLoading(true)
      const data = await documentService.getDocuments()
      setDocuments(data || [])
      setError('')
    } catch (err) {
      setError('Failed to load documents')
      console.error(err)
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async (documentId) => {
    if (window.confirm('Are you sure you want to delete this document?')) {
      try {
        await documentService.deleteDocument(documentId)
        setDocuments(documents.filter((d) => d.id !== documentId))
      } catch (err) {
        setError('Failed to delete document')
      }
    }
  }

  const handleUploadSuccess = () => {
    onClose()
    loadDocuments()
  }

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
  }

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString()
  }

  return (
    <Flex h="100vh" bg={colorMode === 'light' ? 'gray.50' : 'gray.900'}>
      <Box display={{ base: 'none', md: 'block' }} w="180px" bg={colorMode === 'light' ? 'white' : 'gray.800'}>
        <Sidebar onLogout={handleLogout} />
      </Box>

      <Box flex="1" overflowY="auto">
        <HStack p={6} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderBottom="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'} justify="space-between">
          <Heading size="lg">📄 Document Management</Heading>
          <Button colorScheme="blue" onClick={onOpen}>
            + Upload Document
          </Button>
        </HStack>

        <Box p={6}>
          {error && (
            <Alert status="error" mb={4} borderRadius="lg">
              <AlertIcon />
              {error}
            </Alert>
          )}

          {isLoading ? (
            <Flex justify="center" align="center" h="400px">
              <Spinner size="lg" />
            </Flex>
          ) : documents.length === 0 ? (
            <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={8} borderRadius="lg" textAlign="center">
              <Text fontSize="lg" color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
                No documents yet. Upload your first document to get started!
              </Text>
            </Box>
          ) : (
            <TableContainer bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRadius="lg" boxShadow="sm">
              <Table>
                <Thead bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}>
                  <Tr>
                    <Th>File Name</Th>
                    <Th>Type</Th>
                    <Th>Size</Th>
                    <Th>Chunks</Th>
                    <Th>Created</Th>
                    <Th>Actions</Th>
                  </Tr>
                </Thead>
                <Tbody>
                  {documents.map((doc) => (
                    <Tr key={doc.id} borderBottom="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}>
                      <Td fontWeight="500">{doc.fileName}</Td>
                      <Td>
                        <Badge colorScheme="blue">{doc.extension}</Badge>
                      </Td>
                      <Td>{formatFileSize(doc.size)}</Td>
                      <Td>{doc.chunkCount}</Td>
                      <Td>{formatDate(doc.createdAt)}</Td>
                      <Td>
                        <IconButton
                          icon="🗑️"
                          size="sm"
                          variant="ghost"
                          colorScheme="red"
                          onClick={() => handleDelete(doc.id)}
                        />
                      </Td>
                    </Tr>
                  ))}
                </Tbody>
              </Table>
            </TableContainer>
          )}
        </Box>
      </Box>

      <DocumentUploadModal isOpen={isOpen} onClose={onClose} onSuccess={handleUploadSuccess} />
    </Flex>
  )
}

