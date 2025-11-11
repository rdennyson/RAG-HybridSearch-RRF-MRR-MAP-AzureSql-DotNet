import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Flex,
  VStack,
  HStack,
  Heading,
  Text,
  Button,
  useColorMode,
  Drawer,
  DrawerBody,
  DrawerHeader,
  DrawerOverlay,
  DrawerContent,
  useDisclosure,
  Icon,
  Badge,
  Stat,
  StatLabel,
  StatNumber,
  SimpleGrid,
} from '@chakra-ui/react'
import { authService } from '../services/authService'
import Sidebar from '../components/Sidebar'

export default function Dashboard() {
  const navigate = useNavigate()
  const { colorMode } = useColorMode()
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [user, setUser] = useState(null)
  const [stats, setStats] = useState({
    documents: 0,
    categories: 0,
    users: 0,
  })

  useEffect(() => {
    const currentUser = authService.getCurrentUser()
    if (currentUser) {
      setUser(currentUser)
    }
  }, [])

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  return (
    <Flex h="100vh" bg={colorMode === 'light' ? 'gray.50' : 'gray.900'}>
      {/* Desktop Sidebar */}
      <Box display={{ base: 'none', md: 'block' }} w="180px" bg={colorMode === 'light' ? 'white' : 'gray.800'}>
        <Sidebar onLogout={handleLogout} />
      </Box>

      {/* Mobile Drawer */}
      <Drawer isOpen={isOpen} placement="left" onClose={onClose}>
        <DrawerOverlay />
        <DrawerContent>
          <DrawerHeader>Menu</DrawerHeader>
          <DrawerBody>
            <Sidebar onLogout={handleLogout} />
          </DrawerBody>
        </DrawerContent>
      </Drawer>

      {/* Main Content */}
      <Box flex="1" overflowY="auto">
        {/* Header */}
        <HStack
          p={6}
          bg={colorMode === 'light' ? 'white' : 'gray.800'}
          borderBottom="1px"
          borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}
          justify="space-between"
        >
          <HStack>
            <Button display={{ base: 'flex', md: 'none' }} onClick={onOpen} variant="ghost">
              ☰
            </Button>
            <Heading size="lg">Dashboard</Heading>
          </HStack>
          <HStack>
            <Badge colorScheme="blue">{user?.username}</Badge>
          </HStack>
        </HStack>

        {/* Dashboard Content */}
        <Box p={6}>
          <VStack spacing={8} align="stretch">
            {/* Welcome Section */}
            <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={8} borderRadius="lg" boxShadow="sm">
              <Heading size="md" mb={2}>
                Welcome back, {user?.fullName || user?.username}! 👋
              </Heading>
              <Text color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
                Manage your documents, categories, and perform intelligent searches.
              </Text>
            </Box>

            {/* Stats Grid */}
            <SimpleGrid columns={{ base: 1, md: 3 }} spacing={6}>
              <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={6} borderRadius="lg" boxShadow="sm">
                <Stat>
                  <StatLabel>Documents</StatLabel>
                  <StatNumber fontSize="3xl">0</StatNumber>
                </Stat>
              </Box>
              <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={6} borderRadius="lg" boxShadow="sm">
                <Stat>
                  <StatLabel>Categories</StatLabel>
                  <StatNumber fontSize="3xl">0</StatNumber>
                </Stat>
              </Box>
              <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={6} borderRadius="lg" boxShadow="sm">
                <Stat>
                  <StatLabel>Users</StatLabel>
                  <StatNumber fontSize="3xl">0</StatNumber>
                </Stat>
              </Box>
            </SimpleGrid>

            {/* Quick Actions */}
            <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={6} borderRadius="lg" boxShadow="sm">
              <Heading size="sm" mb={4}>
                Quick Actions
              </Heading>
              <HStack spacing={4} wrap="wrap">
                <Button colorScheme="blue" onClick={() => navigate('/documents')}>
                  📄 Manage Documents
                </Button>
                <Button colorScheme="green" onClick={() => navigate('/chat')}>
                  💬 Start Chat
                </Button>
                <Button colorScheme="purple" onClick={() => navigate('/categories')}>
                  🏷️ Manage Categories
                </Button>
                <Button colorScheme="orange" onClick={() => navigate('/users')}>
                  👥 Manage Users
                </Button>
              </HStack>
            </Box>
          </VStack>
        </Box>
      </Box>
    </Flex>
  )
}

