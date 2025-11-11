import { Box, VStack, Button, Text, Heading, Divider, useColorMode, HStack } from '@chakra-ui/react'
import { Link as RouterLink, useLocation } from 'react-router-dom'

export default function Sidebar({ onLogout }) {
  const { colorMode } = useColorMode()
  const location = useLocation()

  const isActive = (path) => location.pathname === path

  const navItems = [
    { label: 'Dashboard', icon: '📊', path: '/dashboard' },
    { label: 'Chat', icon: '💬', path: '/chat' },
    { label: 'Documents', icon: '📄', path: '/documents' },
    { label: 'Categories', icon: '🏷️', path: '/categories' },
    { label: 'Users', icon: '👥', path: '/users' },
  ]

  return (
    <VStack
      h="100%"
      p={3}
      bg={colorMode === 'light' ? 'white' : 'gray.800'}
      color={colorMode === 'light' ? 'gray.900' : 'white'}
      align="stretch"
      spacing={3}
      overflowY="auto"
      borderRight="1px"
      borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'}
    >
      {/* Header */}
      <Box textAlign="center" py={2}>
        <Text fontSize="2xl" mb={0}>
          🗂️
        </Text>
        <Heading size="sm">DocMan</Heading>
      </Box>

      <Divider borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'} my={1} />

      {/* Navigation */}
      <VStack align="stretch" spacing={1} flex={1}>
        {navItems.map((item) => (
          <Button
            key={item.path}
            as={RouterLink}
            to={item.path}
            variant="ghost"
            justifyContent="flex-start"
            bg={isActive(item.path) ? (colorMode === 'light' ? 'blue.50' : 'blue.900') : 'transparent'}
            color={isActive(item.path) ? 'blue.600' : 'inherit'}
            fontWeight={isActive(item.path) ? '600' : '500'}
            fontSize="sm"
            py={2}
            px={3}
            _hover={{ bg: colorMode === 'light' ? 'gray.50' : 'gray.700' }}
            leftIcon={<Text fontSize="lg">{item.icon}</Text>}
          >
            {item.label}
          </Button>
        ))}
      </VStack>

      <Divider borderColor={colorMode === 'light' ? 'gray.200' : 'gray.700'} my={1} />

      {/* Logout */}
      <Button
        w="full"
        colorScheme="red"
        variant="outline"
        onClick={onLogout}
        size="sm"
        fontSize="sm"
      >
        🚪 Logout
      </Button>
    </VStack>
  )
}

