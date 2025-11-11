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
import { userService } from '../services/userService'
import { authService } from '../services/authService'
import Sidebar from '../components/Sidebar'
import UserModal from '../components/UserModal'

export default function UserManagement() {
  const navigate = useNavigate()
  const { colorMode } = useColorMode()
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [users, setUsers] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [editingUser, setEditingUser] = useState(null)

  useEffect(() => {
    loadUsers()
  }, [])

  const loadUsers = async () => {
    try {
      setIsLoading(true)
      const data = await userService.getUsers()
      setUsers(data || [])
      setError('')
    } catch (err) {
      setError('Failed to load users')
      console.error(err)
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async (userId) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await userService.deleteUser(userId)
        setUsers(users.filter((u) => u.id !== userId))
      } catch (err) {
        setError('Failed to delete user')
      }
    }
  }

  const handleEdit = (user) => {
    setEditingUser(user)
    onOpen()
  }

  const handleAddNew = () => {
    setEditingUser(null)
    onOpen()
  }

  const handleSaveSuccess = () => {
    onClose()
    loadUsers()
  }

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString()
  }

  return (
    <Flex h="100vh" bg={colorMode === 'light' ? 'gray.50' : 'gray.900'}>
      <Box display={{ base: 'none', md: 'block' }} w="250px" bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRight="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}>
        <Sidebar onLogout={handleLogout} />
      </Box>

      <Box flex="1" overflowY="auto">
        <HStack p={6} bg={colorMode === 'light' ? 'white' : 'gray.800'} borderBottom="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'} justify="space-between">
          <Heading size="lg">👥 User Management</Heading>
          <Button colorScheme="blue" onClick={handleAddNew}>
            + New User
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
          ) : users.length === 0 ? (
            <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={8} borderRadius="lg" textAlign="center">
              <Text fontSize="lg" color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
                No users found.
              </Text>
            </Box>
          ) : (
            <TableContainer bg={colorMode === 'light' ? 'white' : 'gray.800'} borderRadius="lg" boxShadow="sm">
              <Table>
                <Thead bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}>
                  <Tr>
                    <Th>Username</Th>
                    <Th>Email</Th>
                    <Th>Theme</Th>
                    <Th>Created</Th>
                    <Th>Actions</Th>
                  </Tr>
                </Thead>
                <Tbody>
                  {users.map((user) => (
                    <Tr key={user.id} borderBottom="1px" borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}>
                      <Td fontWeight="500">{user.username}</Td>
                      <Td>{user.email}</Td>
                      <Td>
                        <Badge colorScheme={user.theme === 'dark' ? 'gray' : 'yellow'}>{user.theme}</Badge>
                      </Td>
                      <Td>{formatDate(user.createdAt)}</Td>
                      <Td>
                        <HStack spacing={2}>
                          <Button size="sm" colorScheme="blue" onClick={() => handleEdit(user)}>
                            Edit
                          </Button>
                          <IconButton icon="🗑️" size="sm" variant="ghost" colorScheme="red" onClick={() => handleDelete(user.id)} />
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

      <UserModal isOpen={isOpen} onClose={onClose} user={editingUser} onSuccess={handleSaveSuccess} />
    </Flex>
  )
}

