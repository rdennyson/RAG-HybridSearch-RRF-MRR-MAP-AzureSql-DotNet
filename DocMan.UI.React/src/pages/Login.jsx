import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Button,
  Container,
  FormControl,
  FormLabel,
  Input,
  Stack,
  Text,
  Heading,
  Alert,
  AlertIcon,
  Spinner,
  Link,
  useColorMode,
  VStack,
  HStack,
  Icon,
  Divider,
  FormErrorMessage,
} from '@chakra-ui/react'
import { authService } from '../services/authService'

export default function Login() {
  const [username, setUsername] = useState('john_doe')
  const [password, setPassword] = useState('Password123!')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [usernameError, setUsernameError] = useState('')
  const [passwordError, setPasswordError] = useState('')
  const navigate = useNavigate()
  const { colorMode, toggleColorMode } = useColorMode()

  const validateForm = () => {
    let isValid = true
    setUsernameError('')
    setPasswordError('')

    if (!username.trim()) {
      setUsernameError('Username is required')
      isValid = false
    }

    if (!password) {
      setPasswordError('Password is required')
      isValid = false
    }

    return isValid
  }

  const handleLogin = async (e) => {
    e.preventDefault()
    setError('')

    if (!validateForm()) {
      return
    }

    setIsLoading(true)

    try {
      await authService.login(username, password)
      navigate('/chat')
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid username or password')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Box minH="100vh" bg={colorMode === 'light' ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' : 'gray.900'}>
      <Box position="absolute" top={4} right={4}>
        <Button onClick={toggleColorMode} variant="ghost" color={colorMode === 'light' ? 'white' : 'gray.200'} size="lg">
          {colorMode === 'light' ? '🌙' : '☀️'}
        </Button>
      </Box>

      <Container maxW="sm" centerContent justifyContent="center" minH="100vh">
        <VStack spacing={8} w="full">
          <Box bg={colorMode === 'light' ? 'white' : 'gray.800'} p={10} borderRadius="2xl" boxShadow="2xl" w="full">
            <VStack spacing={8}>
              <VStack spacing={2} textAlign="center">
                <Heading size="2xl" color={colorMode === 'light' ? 'gray.900' : 'white'}>
                  📚 DocMan RAG
                </Heading>
                <Text color={colorMode === 'light' ? 'gray.600' : 'gray.400'} fontSize="md">
                  Intelligent Document Retrieval & Analysis
                </Text>
              </VStack>

              {error && (
                <Alert status="error" borderRadius="lg" bg={colorMode === 'light' ? 'red.50' : 'red.900'}>
                  <AlertIcon />
                  <Box>
                    <Text fontWeight="bold">Login Failed</Text>
                    <Text fontSize="sm">{error}</Text>
                  </Box>
                </Alert>
              )}

              <form onSubmit={handleLogin} style={{ width: '100%' }}>
                <Stack spacing={5}>
                  <FormControl isInvalid={!!usernameError}>
                    <FormLabel fontWeight="600" color={colorMode === 'light' ? 'gray.700' : 'gray.200'}>
                      Username
                    </FormLabel>
                    <Input
                      type="text"
                      placeholder="john_doe"
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      disabled={isLoading}
                      size="lg"
                      bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
                      borderColor={colorMode === 'light' ? 'gray.200' : 'gray.600'}
                      _focus={{ borderColor: 'blue.500', boxShadow: '0 0 0 1px blue.500' }}
                    />
                    {usernameError && <FormErrorMessage>{usernameError}</FormErrorMessage>}
                  </FormControl>

                  <FormControl isInvalid={!!passwordError}>
                    <FormLabel fontWeight="600" color={colorMode === 'light' ? 'gray.700' : 'gray.200'}>
                      Password
                    </FormLabel>
                    <Input
                      type="password"
                      placeholder="••••••••"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      disabled={isLoading}
                      size="lg"
                      bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
                      borderColor={colorMode === 'light' ? 'gray.200' : 'gray.600'}
                      _focus={{ borderColor: 'blue.500', boxShadow: '0 0 0 1px blue.500' }}
                    />
                    {passwordError && <FormErrorMessage>{passwordError}</FormErrorMessage>}
                  </FormControl>

                  <Button
                    type="submit"
                    colorScheme="blue"
                    size="lg"
                    w="full"
                    isDisabled={isLoading}
                    isLoading={isLoading}
                    loadingText="Logging in..."
                    fontWeight="600"
                    mt={2}
                  >
                    Sign In
                  </Button>
                </Stack>
              </form>

              <Divider />

              <VStack spacing={3} w="full" fontSize="sm" color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
                <Text>Demo Credentials:</Text>
                <HStack spacing={4} justify="center" fontSize="xs">
                  <Box>
                    <Text fontWeight="600">Username:</Text>
                    <Text>john_doe</Text>
                  </Box>
                  <Box>
                    <Text fontWeight="600">Password:</Text>
                    <Text>Password123!</Text>
                  </Box>
                </HStack>
              </VStack>
            </VStack>
          </Box>
        </VStack>
      </Container>
    </Box>
  )
}

