import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Button,
  Flex,
  HStack,
  Heading,
  useColorMode,
  Drawer,
  DrawerBody,
  DrawerHeader,
  DrawerOverlay,
  DrawerContent,
  useDisclosure,
} from '@chakra-ui/react'
import { authService } from '../services/authService'
import Sidebar from '../components/Sidebar'
import ChatInterface from '../components/ChatInterface'

export default function Chat() {
  const [user, setUser] = useState(null)
  const navigate = useNavigate()
  const { colorMode, toggleColorMode } = useColorMode()
  const { isOpen, onOpen, onClose } = useDisclosure()

  useEffect(() => {
    const currentUser = authService.getCurrentUser()
    if (!currentUser) {
      navigate('/login')
    } else {
      setUser(currentUser)
    }
  }, [navigate])

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

      {/* Main Chat Area */}
      <Flex direction="column" flex={1}>
        {/* Header */}
        <HStack
          p={4}
          bg={colorMode === 'light' ? 'white' : 'gray.800'}
          borderBottom="1px"
          borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}
          justify="space-between"
        >
          <HStack>
            <Button display={{ base: 'flex', md: 'none' }} onClick={onOpen} variant="ghost" size="lg">
              ☰
            </Button>
            <Heading size="lg">💬 Document Search & Analysis</Heading>
          </HStack>
          <Button onClick={toggleColorMode} variant="ghost" size="lg">
            {colorMode === 'light' ? '🌙' : '☀️'}
          </Button>
        </HStack>

        {/* Chat Interface */}
        <ChatInterface />
      </Flex>
    </Flex>
  )
}

