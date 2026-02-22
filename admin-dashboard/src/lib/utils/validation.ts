export function validateEmail(email: string): boolean {
	const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
	return emailRegex.test(email);
}

export function validatePhone(phone: string): boolean {
	const phoneRegex = /^[\+]?[0-9\s\-\(\)]{8,}$/;
	return phoneRegex.test(phone);
}

export function validateRequired(value: string | number | null | undefined): boolean {
	return value !== null && value !== undefined && value !== '';
}

export function validateMinLength(value: string, minLength: number): boolean {
	return value.length >= minLength;
}

export function validateMaxLength(value: string, maxLength: number): boolean {
	return value.length <= maxLength;
}

export function validateDateRange(startDate: string, endDate: string): boolean {
	const start = new Date(startDate);
	const end = new Date(endDate);
	return start <= end;
}

export function validateFutureDate(date: string): boolean {
	const inputDate = new Date(date);
	const today = new Date();
	today.setHours(0, 0, 0, 0);
	return inputDate >= today;
}

export function validateBookingDates(startDate: string, endDate: string): { isValid: boolean; error?: string } {
	if (!validateRequired(startDate) || !validateRequired(endDate)) {
		return { isValid: false, error: 'Start- og slutdato er påkrævet' };
	}

	if (!validateDateRange(startDate, endDate)) {
		return { isValid: false, error: 'Slutdato skal være efter startdato' };
	}

	if (!validateFutureDate(startDate)) {
		return { isValid: false, error: 'Startdato skal være i dag eller senere' };
	}

	return { isValid: true };
}

export function validateRoomCapacity(capacity: number): boolean {
	return capacity > 0 && capacity <= 20;
}

export function validateBookingForm(data: {
	userId: string;
	roomId: string;
	startDate: string;
	endDate: string;
}): { isValid: boolean; errors: Record<string, string> } {
	const errors: Record<string, string> = {};

	if (!validateRequired(data.userId)) {
		errors.userId = 'Bruger er påkrævet';
	}

	if (!validateRequired(data.roomId)) {
		errors.roomId = 'Værelse er påkrævet';
	}

	const dateValidation = validateBookingDates(data.startDate, data.endDate);
	if (!dateValidation.isValid) {
		errors.dates = dateValidation.error || 'Ugyldig dato';
	}

	return {
		isValid: Object.keys(errors).length === 0,
		errors
	};
}
