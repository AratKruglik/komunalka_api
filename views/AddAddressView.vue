<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import TextInput from '../UI/forms/TextInput.vue'
import PrimaryButton from '../UI/buttons/PrimaryButton.vue'
import { 
  HomeIcon, 
  CheckIcon,
  ChevronRightIcon,
  ChevronDownIcon,
  BuildingOfficeIcon,
  ExclamationTriangleIcon
} from '@heroicons/vue/24/outline'
import { 
  BuildingOffice2Icon
} from '@heroicons/vue/24/solid'

const router = useRouter()

// Form data
const propertyType = ref('')
const region = ref('')
const city = ref('')
const street = ref('')
const houseNumber = ref('')
const apartmentNumber = ref('')
const postalCode = ref('')
const additionalNotes = ref('')
const isPrimaryAddress = ref(false)

// Form validation
const errors = ref({})

// Region dropdown
const isRegionDropdownOpen = ref(false)
const regions = [
  'Київська область',
  'Львівська область',
  'Харківська область',
  'Одеська область',
  'Дніпропетровська область'
]

const selectRegion = (selectedRegion) => {
  region.value = selectedRegion
  isRegionDropdownOpen.value = false
  clearFieldError('region')
}

// Validation functions
const validateForm = () => {
  errors.value = {}
  let isValid = true

  const requiredFields = [
    { field: 'propertyType', message: "Оберіть тип нерухомості" },
    { field: 'region', message: "Оберіть область" },
    { field: 'city', message: "Введіть назву міста" },
    { field: 'street', message: "Введіть назву вулиці" },
    { field: 'houseNumber', message: "Введіть номер будинку" },
    { field: 'apartmentNumber', message: "Введіть номер квартири/офісу" },
    { field: 'postalCode', message: "Введіть поштовий індекс" }
  ]

  requiredFields.forEach(({ field, message }) => {
    const value = eval(field + '.value')
    if (!value) {
      errors.value[field] = message
      isValid = false
    }
  })

  // Special validation for postal code
  if (postalCode.value && !/^\d{5}$/.test(postalCode.value)) {
    errors.value.postalCode = "Поштовий індекс повинен містити 5 цифр"
    isValid = false
  }

  return isValid
}

const clearFieldError = (field) => {
  if (errors.value[field]) {
    delete errors.value[field]
  }
}

const handleSubmit = () => {
  if (validateForm()) {
    // Handle form submission
    console.log('Form submitted', {
      propertyType: propertyType.value,
      region: region.value,
      city: city.value,
      street: street.value,
      houseNumber: houseNumber.value,
      apartmentNumber: apartmentNumber.value,
      postalCode: postalCode.value,
      additionalNotes: additionalNotes.value,
      isPrimaryAddress: isPrimaryAddress.value
    })

    // Navigate back or show success message
    router.push('/')
  }
}

const handleCancel = () => {
  router.back()
}

// Property type options
const propertyTypes = [
  { value: 'apartment', label: 'Квартира', icon: BuildingOffice2Icon },
  { value: 'house', label: 'Приватний будинок', icon: HomeIcon },
  { value: 'office', label: 'Офіс', icon: BuildingOfficeIcon }
]
</script>
