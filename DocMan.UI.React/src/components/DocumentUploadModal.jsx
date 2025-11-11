import { useState, useRef } from 'react'
import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  ModalCloseButton,
  Button,
  VStack,
  Box,
  Text,
  Progress,
  Alert,
  AlertIcon,
  useColorMode,
  FormControl,
  FormLabel,
  Select,
} from '@chakra-ui/react'
import { documentService } from '../services/documentService'
import { categoryService } from '../services/categoryService'
import { useEffect } from 'react'

const ALLOWED_EXTENSIONS = ['.pdf', '.docx', '.txt', '.md']
const MAX_FILE_SIZE = 50 * 1024 * 1024 // 50MB

export default function DocumentUploadModal({ isOpen, onClose, onSuccess }) {
  const { colorMode } = useColorMode()
  const [file, setFile] = useState(null)
  const [isDragging, setIsDragging] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [uploadProgress, setUploadProgress] = useState(0)
  const [error, setError] = useState('')
  const [categories, setCategories] = useState([])
  const [selectedCategory, setSelectedCategory] = useState('')
  const fileInputRef = useRef(null)

  useEffect(() => {
    if (isOpen) {
      loadCategories()
    }
  }, [isOpen])

  const loadCategories = async () => {
    try {
      const data = await categoryService.getCategories()
      setCategories(data || [])
    } catch (err) {
      console.error('Failed to load categories:', err)
    }
  }

  const validateFile = (selectedFile) => {
    setError('')

    const extension = '.' + selectedFile.name.split('.').pop().toLowerCase()
    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      setError(`Invalid file type. Allowed types: ${ALLOWED_EXTENSIONS.join(', ')}`)
      return false
    }

    if (selectedFile.size > MAX_FILE_SIZE) {
      setError(`File size exceeds 50MB limit`)
      return false
    }

    return true
  }

  const handleDragEnter = (e) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(true)
  }

  const handleDragLeave = (e) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)
  }

  const handleDrop = (e) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)

    const droppedFiles = e.dataTransfer.files
    if (droppedFiles.length > 0) {
      const selectedFile = droppedFiles[0]
      if (validateFile(selectedFile)) {
        setFile(selectedFile)
      }
    }
  }

  const handleFileSelect = (e) => {
    const selectedFile = e.target.files?.[0]
    if (selectedFile && validateFile(selectedFile)) {
      setFile(selectedFile)
    }
  }

  const handleUpload = async () => {
    if (!file) {
      setError('Please select a file')
      return
    }

    setIsUploading(true)
    setUploadProgress(0)

    try {
      await documentService.uploadDocument(file, selectedCategory || null)
      setUploadProgress(100)
      setTimeout(() => {
        setFile(null)
        setSelectedCategory('')
        setUploadProgress(0)
        onSuccess()
        onClose()
      }, 1000)
    } catch (err) {
      setError(err.response?.data?.message || 'Upload failed')
    } finally {
      setIsUploading(false)
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <ModalOverlay />
      <ModalContent bg={colorMode === 'light' ? 'white' : 'gray.800'}>
        <ModalHeader>Upload Document</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <VStack spacing={6}>
            {error && (
              <Alert status="error" borderRadius="lg">
                <AlertIcon />
                {error}
              </Alert>
            )}

            {/* Drag and Drop Area */}
            <Box
              w="full"
              p={8}
              border="2px dashed"
              borderColor={isDragging ? 'blue.500' : colorMode === 'light' ? 'gray.300' : 'gray.600'}
              borderRadius="lg"
              bg={isDragging ? (colorMode === 'light' ? 'blue.50' : 'blue.900') : colorMode === 'light' ? 'gray.50' : 'gray.700'}
              textAlign="center"
              cursor="pointer"
              transition="all 0.2s"
              onDragEnter={handleDragEnter}
              onDragLeave={handleDragLeave}
              onDragOver={(e) => e.preventDefault()}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
            >
              <VStack spacing={3}>
                <Text fontSize="3xl">📁</Text>
                <Text fontWeight="600">Drag and drop your file here</Text>
                <Text fontSize="sm" color={colorMode === 'light' ? 'gray.600' : 'gray.400'}>
                  or click to select
                </Text>
                <Text fontSize="xs" color={colorMode === 'light' ? 'gray.500' : 'gray.500'}>
                  Supported: PDF, DOCX, TXT, MD (Max 50MB)
                </Text>
              </VStack>
              <input
                ref={fileInputRef}
                type="file"
                hidden
                accept=".pdf,.docx,.txt,.md"
                onChange={handleFileSelect}
              />
            </Box>

            {/* Selected File Info */}
            {file && (
              <Box w="full" p={4} bg={colorMode === 'light' ? 'green.50' : 'green.900'} borderRadius="lg" borderLeft="4px" borderColor="green.500">
                <Text fontWeight="600" color="green.700">
                  ✓ {file.name}
                </Text>
                <Text fontSize="sm" color={colorMode === 'light' ? 'green.600' : 'green.300'}>
                  {(file.size / 1024 / 1024).toFixed(2)} MB
                </Text>
              </Box>
            )}

            {/* Category Selection */}
            <FormControl>
              <FormLabel>Category (Optional)</FormLabel>
              <Select
                placeholder="Select a category"
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                bg={colorMode === 'light' ? 'gray.50' : 'gray.700'}
              >
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </Select>
            </FormControl>

            {/* Upload Progress */}
            {isUploading && (
              <Box w="full">
                <Progress value={uploadProgress} size="lg" colorScheme="blue" borderRadius="lg" />
                <Text fontSize="sm" textAlign="center" mt={2}>
                  {uploadProgress}% uploaded
                </Text>
              </Box>
            )}
          </VStack>
        </ModalBody>
        <ModalFooter>
          <Button variant="ghost" mr={3} onClick={onClose} isDisabled={isUploading}>
            Cancel
          </Button>
          <Button colorScheme="blue" onClick={handleUpload} isDisabled={!file || isUploading} isLoading={isUploading}>
            Upload
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  )
}

