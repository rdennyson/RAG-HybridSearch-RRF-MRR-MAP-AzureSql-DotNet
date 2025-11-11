import { useState, useEffect } from 'react'
import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  ModalCloseButton,
  Button,
  FormControl,
  FormLabel,
  Input,
  useColorMode,
  Alert,
  AlertIcon,
  FormErrorMessage,
} from '@chakra-ui/react'
import { userService } from '../services/userService'

export default function UserModal({ isOpen, onClose, user, onSuccess }) {
  const { colorMode } = useColorMode()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [emailError, setEmailError] = useState('')

  useEffect(() => {
    if (user) {
      setUsername(user.username)
      setEmail(user.email)
      setPassword('')
    } else {
      setUsername('')
      setEmail('')
      setPassword('')
    }
    setError('')
    setEmailError('')
  }, [user, isOpen])

  const validateEmail = (email) => {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return re.test(email)
  }

  const handleSave = async () => {
    setError('')
    setEmailError('')

    if (!username.trim()) {
      setError('Username is required')
      return
    }

    if (!email.trim()) {
      setError('Email is required')
      return
    }

    if (!validateEmail(email)) {
      setEmailError('Invalid email format')
      return
    }

    if (!user && !password) {
      setError('Password is required for new users')
      return
    }

    setIsLoading(true)
    try {
      if (user) {
        await userService.updateUser(user.id, email)
      } else {
        await userService.createUser(username, email, password)
      }
      onSuccess()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save user')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <ModalOverlay />
      <ModalContent bg={colorMode === 'light' ? 'white' : 'gray.800'}>
        <ModalHeader>{user ? 'Edit User' : 'Create New User'}</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          {error && (
            <Alert status="error" mb={4} borderRadius="lg">
              <AlertIcon />
              {error}
            </Alert>
          )}

          <FormControl mb={4} isDisabled={!!user}>
            <FormLabel>Username</FormLabel>
            <Input
              placeholder="john_doe"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
              isDisabled={!!user}
            />
          </FormControl>

          <FormControl mb={4} isInvalid={!!emailError}>
            <FormLabel>Email</FormLabel>
            <Input
              type="email"
              placeholder="user@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
            />
            {emailError && <FormErrorMessage>{emailError}</FormErrorMessage>}
          </FormControl>

          {!user && (
            <FormControl mb={4}>
              <FormLabel>Password</FormLabel>
              <Input
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
              />
            </FormControl>
          )}
        </ModalBody>
        <ModalFooter>
          <Button variant="ghost" mr={3} onClick={onClose} isDisabled={isLoading}>
            Cancel
          </Button>
          <Button colorScheme="blue" onClick={handleSave} isLoading={isLoading}>
            {user ? 'Update' : 'Create'}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  )
}

