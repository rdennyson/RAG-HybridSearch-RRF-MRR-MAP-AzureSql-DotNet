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
import { categoryService } from '../services/categoryService'
import { authService } from '../services/authService'
import Sidebar from '../components/Sidebar'
import CategoryModal from '../components/CategoryModal'

export default function CategoryManagement() {
  const navigate = useNavigate()
  const { colorMode } = useColorMode()
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [categories, setCategories] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [editingCategory, setEditingCategory] = useState(null)

  useEffect(() => {
    loadCategories()
  }, [])

  const loadCategories = async () => {
    try {
      setIsLoading(true)
      const data = await categoryService.getCategories()
      setCategories(data || [])
      setError('')
    } catch (err) {
      setError('Failed to load categories')
      console.error(err)
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async (categoryId) => {
    if (window.confirm('Are you sure you want to delete this category?')) {
      try {
        await categoryService.deleteCategory(categoryId)
        setCategories(categories.filter((c) => c.id !== categoryId))
      } catch (err) {
        setError('Failed to delete category')
      }
    }
  }

  const handleEdit = (category) => {
    setEditingCategory(category)
    onOpen()
  }

  const handleAddNew = () => {
    setEditingCategory(null)
    onOpen()
  }

  const handleSaveSuccess = () => {
    onClose()
    loadCategories()
  }

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  return (
    <Flex h="100vh" bg={colorMode === 'light' ? 'gray.50' : 'gray.900'}>
      <Box display={{ base: 'none', md: 'block' }} w="180px" bg={colorMode === 'light' ? 'white' : 'gray.800'}>
        <Sidebar onLogout={handleLogout} />
      </Box>

      <Box flex="1" overflowY="auto">
        <HStack p={6} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderBottom="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'} justify="space-between">
          <Heading size="lg">🏷️ Category Management</Heading>
          <Button colorScheme="blue" onClick={handleAddNew}>
            + New Category
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
          ) : categories.length === 0 ? (
            <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={8} borderRadius="lg" textAlign="center">
              <Text fontSize="lg" color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
                No categories yet. Create your first category!
              </Text>
            </Box>
          ) : (
            <TableContainer bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRadius="lg" boxShadow="sm">
              <Table>
                <Thead bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}>
                  <Tr>
                    <Th>Name</Th>
                    <Th>Description</Th>
                    <Th>Color</Th>
                    <Th>Actions</Th>
                  </Tr>
                </Thead>
                <Tbody>
                  {categories.map((cat) => (
                    <Tr key={cat.id} borderBottom="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}>
                      <Td fontWeight="500">{cat.name}</Td>
                      <Td>{cat.description}</Td>
                      <Td>
                        <HStack>
                          <Box w={6} h={6} bg={cat.color} borderRadius="md" />
                          <Text fontSize="sm">{cat.color}</Text>
                        </HStack>
                      </Td>
                      <Td>
                        <HStack spacing={2}>
                          <Button size="sm" colorScheme="blue" onClick={() => handleEdit(cat)}>
                            Edit
                          </Button>
                          <IconButton icon="🗑️" size="sm" variant="ghost" colorScheme="red" onClick={() => handleDelete(cat.id)} />
                        </HStack>
                      </Td>
                    </Tr>
                  ))}
                </Tbody>
              </Table>
            </TableContainer>
          )}
        </Box>
      </Box>

      <CategoryModal isOpen={isOpen} onClose={onClose} category={editingCategory} onSuccess={handleSaveSuccess} />
    </Flex>
  )
}

