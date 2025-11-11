import { Box, Container, Heading, Text, VStack, Button, useColorMode, HStack } from '@chakra-ui/react'
import { useNavigate } from 'react-router-dom'
import { authService } from '../services/authService'

export default function Documents() {
  const navigate = useNavigate()
  const { colorMode, toggleColorMode } = useColorMode()

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  return (
    <Box minH="100vh" bg="gray.50" _dark={{ bg: 'gray.900' }}>
      <HStack p={4} bg="white" _dark={{ bg: 'gray.800' }} justify="space-between">
        <Heading size="lg">Documents</Heading>
        <Button onClick={handleLogout} colorScheme="red" size="sm">
          Logout
        </Button>
      </HStack>

      <Container maxW="container.lg" py={8}>
        <VStack spacing={4} align="start">
          <Heading size="md">Upload Documents</Heading>
          <Text color="gray.600">Document upload functionality coming soon...</Text>
          <Button colorScheme="brand" onClick={() => navigate('/chat')}>
            Back to Chat
          </Button>
        </VStack>
      </Container>
    </Box>
  )
}

