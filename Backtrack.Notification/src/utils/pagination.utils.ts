// import { PaginationOptions } from '@src/types/shared.type'

// type SortOrder = 'asc' | 'desc'

// export class PaginationUtils<T extends PaginationOptions> {
//   public computePaging(docs: T[], limit: number) {
//     let hasMore = false
//     if (docs.length > limit) hasMore = true

//     let items = docs
//     if (hasMore) items = docs.slice(0, limit)

//     return { hasMore, items }
//   }

//   public getCursorValue(lastItem: T, sortField: string) {
//     if (!lastItem) return null

//     const value = lastItem[sortField]
//     if (value === undefined || value === null) return null

//     if (value instanceof Date) return value.toISOString()
//     return value.toString()
//   }

//   public buildFilter(options: T, request: Omit<T, keyof PaginationOptions>) {
//     for (const key in request) {
//       const value = request[key]
//       if (value !== undefined && value !== null) {
//         options[key] = value
//       }
//     }

//     const {
//       userId,
//       channel,
//       status,
//       isRead,
//       isArchived,
//       cursor,
//       sortField = 'createdAt',
//       sortOrder = 'desc',
//     } = request

//     const filter: Record<string, any> = { userId }

//     if (channel) filter.channel = channel
//     if (status) filter.status = status
//     if (isRead !== undefined) filter.isRead = isRead
//     if (isArchived !== undefined) filter.isArchived = isArchived

//     if (cursor) {
//       const cursorDate = new Date(cursor)
//       const op = sortOrder === 'asc' ? '$gt' : '$lt'
//       filter[sortField] = { [op]: cursorDate }
//     }

//     return filter
//   }

//   public buildSort(sortField: string, sortOrder: SortOrder) {
//     let direction: 1 | -1 = 1
//     if (sortOrder === 'desc') direction = -1

//     return { [sortField]: direction }
//   }
// }
