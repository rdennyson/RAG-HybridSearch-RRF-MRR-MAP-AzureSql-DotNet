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
  Textarea,
  HStack,
  Box,
  useColorMode,
  Alert,
  AlertIcon,
} from '@chakra-ui/react'
import { categoryService } from '../services/categoryService'

const PRESET_COLORS = ['#3182CE', '#38A169', '#D69E2E', '#C53030', '#805AD5', '#0891B2', '#EC4899', '#F59E0B']

export default function CategoryModal({ isOpen, onClose, category, onSuccess }) {
  const { colorMode } = useColorMode()
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [color, setColor] = useState('#3182CE')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (category) {
      setName(category.name)
      setDescription(category.description || '')
      setColor(category.color || '#3182CE')
    } else {
      setName('')
      setDescription('')
      setColor('#3182CE')
    }
    setError('')
  }, [category, isOpen])

  const handleSave = async () => {
    if (!name.trim()) {
      setError('Category name is required')
      return
    }

    setIsLoading(true)
    try {
      if (category) {
        await categoryService.updateCategory(category.id, name, description, color)
      } else {
        await categoryService.createCategory(name, description, color)
      }
      onSuccess()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save category')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <ModalOverlay />
      <ModalContent bg={colorMode === 'light' ? 'white' : 'gray.800'}>
        <ModalHeader>{category ? 'Edit Category' : 'Create New Category'}</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          {error && (
            <Alert status="error" mb={4} borderRadius="lg">
              <AlertIcon />
              {error}
            </Alert>
          )}

          <FormControl mb={4}>
            <FormLabel>Category Name</FormLabel>
            <Input
              placeholder="e.g., Finance, Legal, HR"
              value={name}
              onChange={(e) => setName(e.target.value)}
              bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
            />
          </FormControl>

          <FormControl mb={4}>
            <FormLabel>Description</FormLabel>
            <Textarea
              placeholder="Optional description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
              rows={3}
            />
          </FormControl>

          <FormControl>
            <FormLabel>Color</FormLabel>
            <HStack spacing={2} mb={3}>
              {PRESET_COLORS.map((c) => (
                <Box
                  key={c}
                  w={8}
                  h={8}
                  bg={c}
                  borderRadius="md"
                  cursor="pointer"
                  border={color === c ? '3px solid' : 'none'}
                  borderColor="white"
                  boxShadow={color === c ? '0 0 0 2px blue' : 'none'}
                  onClick={() => setColor(c)}
                />
              ))}
            </HStack>
            <Input
              type="color"
              value={color}
              onChange={(e) => setColor(e.target.value)}
              h={10}
            />
          </FormControl>
        </ModalBody>
        <ModalFooter>
          <Button variant="ghost" mr={3} onClick={onClose} isDisabled={isLoading}>
            Cancel
          </Button>
          <Button colorScheme="blue" onClick={handleSave} isLoading={isLoading}>
            {category ? 'Update' : 'Create'}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  )
}

