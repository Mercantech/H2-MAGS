import { format, parseISO, isToday, isTomorrow, isYesterday, addDays, startOfDay, endOfDay } from 'date-fns';
import { da } from 'date-fns/locale';

export function formatDate(date: string | Date, formatStr: string = 'dd/MM/yyyy'): string {
	const dateObj = typeof date === 'string' ? parseISO(date) : date;
	return format(dateObj, formatStr, { locale: da });
}

export function formatDateTime(date: string | Date): string {
	const dateObj = typeof date === 'string' ? parseISO(date) : date;
	return format(dateObj, 'dd/MM/yyyy HH:mm', { locale: da });
}

export function formatTime(date: string | Date): string {
	const dateObj = typeof date === 'string' ? parseISO(date) : date;
	return format(dateObj, 'HH:mm', { locale: da });
}

export function getRelativeDate(date: string | Date): string {
	const dateObj = typeof date === 'string' ? parseISO(date) : date;
	
	if (isToday(dateObj)) {
		return 'I dag';
	} else if (isTomorrow(dateObj)) {
		return 'I morgen';
	} else if (isYesterday(dateObj)) {
		return 'I går';
	} else {
		return formatDate(dateObj);
	}
}

export function getDateRange(startDate: string | Date, endDate: string | Date): string {
	const start = typeof startDate === 'string' ? parseISO(startDate) : startDate;
	const end = typeof endDate === 'string' ? parseISO(endDate) : endDate;
	
	const startFormatted = formatDate(start);
	const endFormatted = formatDate(end);
	
	if (startFormatted === endFormatted) {
		return startFormatted;
	}
	
	return `${startFormatted} - ${endFormatted}`;
}

export function getTodayString(): string {
	return format(new Date(), 'yyyy-MM-dd');
}

export function getTomorrowString(): string {
	return format(addDays(new Date(), 1), 'yyyy-MM-dd');
}

export function getWeekFromToday(): string {
	return format(addDays(new Date(), 7), 'yyyy-MM-dd');
}

export function isDateInRange(date: string | Date, startDate: string | Date, endDate: string | Date): boolean {
	const checkDate = typeof date === 'string' ? parseISO(date) : date;
	const start = typeof startDate === 'string' ? parseISO(startDate) : startDate;
	const end = typeof endDate === 'string' ? parseISO(endDate) : endDate;
	
	return checkDate >= start && checkDate <= end;
}

export function getStartOfDay(date: string | Date): Date {
	const dateObj = typeof date === 'string' ? parseISO(date) : date;
	return startOfDay(dateObj);
}

export function getEndOfDay(date: string | Date): Date {
	const dateObj = typeof date === 'string' ? parseISO(date) : date;
	return endOfDay(dateObj);
}
