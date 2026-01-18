# Image Upload & Tag System - Quick Developer Guide

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** Image Upload & Tag System - Quick Developer Guide  
> **Description:** Quick technical reference for developers implementing or extending multi-image upload and tag system features.  
> **Audience:** developer, backend developer, frontend developer  
> **Topic:** technical  
> **Last Update:** 2026-01-18
>
> ### 📌 Recommended
> **Parent Document:** [architecture-overview.md](./architecture-overview.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 15 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ Optional
> **Prerequisites:** Cloudinary integration knowledge, REST API basics  
> **Related Docs:** [cloudinary-integration.md](./cloudinary-integration.md), [../development/content-management-features.md](../development/content-management-features.md)  
> **Tags:** `api`, `image-upload`, `tags`, `rest-api`, `cloudinary`

---

## 📋 Overview

This is a quick reference guide for developers working with the multi-image upload and tag system APIs. Contains code examples, endpoint reference, and common patterns.

---

## 🚀 Quick Start: Multi-Image Upload

### Backend: Create Post with Images

**Endpoint:**
```http
POST /posts HTTP/1.1
Content-Type: multipart/form-data
```

**Request (FormData):**
```javascript
const formData = new FormData();
formData.append('title', 'My Blog Post');
formData.append('content', 'Post content...');
formData.append('author', 'John Doe');
formData.append('images', file1);  // File object
formData.append('images', file2);  // Multiple files
formData.append('images', file3);
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "My Blog Post",
  "content": "Post content...",
  "imageUrls": [
    "https://res.cloudinary.com/...signed-url-1...",
    "https://res.cloudinary.com/...signed-url-2...",
    "https://res.cloudinary.com/...signed-url-3..."
  ],
  "createdAt": "2026-01-18T12:00:00Z"
}
```

### Frontend: Upload Implementation

**React Component Pattern:**
```typescript
// SimpleBlog.Web/client/src/components/posts/PostForm.tsx

const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
const [previewUrls, setPreviewUrls] = useState<string[]>([]);

const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
  const files = Array.from(event.currentTarget.files || []);
  
  // Validate files
  files.forEach(file => {
    if (file.size > 10 * 1024 * 1024) {
      alert('File too large (max 10MB)');
      return;
    }
  });
  
  setSelectedFiles(files);
  
  // Create previews
  const urls = files.map(file => URL.createObjectURL(file));
  setPreviewUrls(urls);
};

const handleRemovePreview = (index: number) => {
  URL.revokeObjectURL(previewUrls[index]);
  setSelectedFiles(prev => prev.filter((_, i) => i !== index));
  setPreviewUrls(prev => prev.filter((_, i) => i !== index));
};

// When submitting, pass files to API
const handleSubmit = async (data: CreatePostRequest) => {
  await postsApi.create(data, selectedFiles);
};
```

**API Client Pattern:**
```typescript
// SimpleBlog.Web/client/src/api/posts.ts

export async function create(request: CreatePostRequest, files?: File[]): Promise<Post> {
  if (files && files.length > 0) {
    const formData = new FormData();
    formData.append('title', request.title);
    formData.append('content', request.content);
    if (request.author) formData.append('author', request.author);
    
    files.forEach(file => formData.append('images', file));
    
    return apiClient.post('/posts', formData);
  }
  
  return apiClient.post('/posts', request);
}
```

---

## 🏷️ Quick Start: Tag System

### Backend: CRUD Operations

**Create Tag:**
```http
POST /tags HTTP/1.1
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "ASP.NET Core",
  "color": "#512BD4"
}
```

**Response:**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "name": "ASP.NET Core",
  "slug": "asp-net-core",
  "color": "#512BD4",
  "createdAt": "2026-01-18T12:00:00Z"
}
```

**List Tags:**
```http
GET /tags HTTP/1.1

Response: [{ id, name, slug, color, createdAt }, ...]
```

**Assign Tags to Post:**
```http
PUT /posts/{postId}/tags HTTP/1.1
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "tagIds": [
    "660e8400-e29b-41d4-a716-446655440000",
    "770e8400-e29b-41d4-a716-446655440000"
  ]
}
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "My Post",
  "tags": [
    { "id": "660e8400-e29b-41d4-a716-446655440000", "name": "ASP.NET Core", ... },
    { "id": "770e8400-e29b-41d4-a716-446655440000", "name": "Cloudinary", ... }
  ]
}
```

### Frontend: Tag Component Pattern

**Tag Input (Multi-Select):**
```typescript
// Future implementation

interface TagInputProps {
  selectedTagIds: Guid[];
  onChange: (tagIds: Guid[]) => void;
  availableTags: Tag[];
}

export function TagInput({ selectedTagIds, onChange, availableTags }: TagInputProps) {
  const handleTagToggle = (tagId: Guid) => {
    const updated = selectedTagIds.includes(tagId)
      ? selectedTagIds.filter(id => id !== tagId)
      : [...selectedTagIds, tagId];
    onChange(updated);
  };

  return (
    <div className="tag-input">
      {availableTags.map(tag => (
        <label key={tag.id} className="tag-checkbox">
          <input
            type="checkbox"
            checked={selectedTagIds.includes(tag.id)}
            onChange={() => handleTagToggle(tag.id)}
          />
          <span className="tag-label" style={{ backgroundColor: tag.color }}>
            {tag.name}
          </span>
        </label>
      ))}
    </div>
  );
}
```

**Tag Display (Badges):**
```typescript
interface TagBadgesProps {
  tags: Tag[];
  clickable?: boolean;
  onTagClick?: (tag: Tag) => void;
}

export function TagBadges({ tags, clickable, onTagClick }: TagBadgesProps) {
  return (
    <div className="tag-badges">
      {tags.map(tag => (
        <span
          key={tag.id}
          className={`badge ${clickable ? 'badge-clickable' : ''}`}
          style={{ backgroundColor: tag.color || '#6c757d' }}
          onClick={() => clickable && onTagClick?.(tag)}
        >
          {tag.name}
        </span>
      ))}
    </div>
  );
}
```

---

## 📚 API Endpoint Reference

### Image Management Endpoints

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/posts` | POST | Admin | Create post with images |
| `/posts/{id}/images` | POST | Admin | Add images to existing post |
| `/posts/{id}/images` | DELETE | Admin | Remove image from post |
| `/products` | POST | Admin | Create product with images |
| `/products/{id}/images` | POST | Admin | Add images to product |
| `/products/{id}/images` | DELETE | Admin | Remove image from product |

### Tag Endpoints

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/tags` | GET | Public | List all tags |
| `/tags/{id}` | GET | Public | Get tag by ID |
| `/tags/by-slug/{slug}` | GET | Public | Get tag by slug |
| `/tags` | POST | Admin | Create tag |
| `/tags/{id}` | PUT | Admin | Update tag |
| `/tags/{id}` | DELETE | Admin | Delete tag |
| `/posts/{id}/tags` | PUT | Admin | Assign tags to post |
| `/products/{id}/tags` | PUT | Admin | Assign tags to product |

---

## 🔧 Common Implementation Patterns

### Pattern 1: Create Post with Images

**Backend Flow:**
1. Client sends multipart/form-data (fields + files)
2. Endpoint validates files (size, type, count)
3. Post entity created in database
4. Images uploaded to Cloudinary `posts/` folder
5. Cloudinary returns `cloudinary://public-id` format
6. Post updated with image URLs
7. Before returning, convert to signed URLs (60-min expiration)

**Code:**
```csharp
// SimpleBlog.ApiService/Endpoints/PostEndpoints.cs

private static async Task<IResult> Create(
    HttpContext context,
    IPostRepository repository,
    IImageStorageService imageStorage,
    ILogger<Program> logger)
{
    try
    {
        // Check if multipart or JSON
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            var request = new CreatePostRequest(
                form["title"],
                form["content"],
                form["author"]
            );

            // Upload files
            var uploadedFiles = form.Files["images"];
            var imageUrls = new List<string>();

            foreach (var file in uploadedFiles)
            {
                using var stream = file.OpenReadStream();
                var cloudinaryUrl = await imageStorage.UploadImageAsync(
                    stream,
                    file.FileName,
                    "posts");
                imageUrls.Add(cloudinaryUrl);
            }

            // Create post with images
            var post = await repository.CreateAsync(request);
            foreach (var url in imageUrls)
            {
                await repository.AddImageAsync(post.Id, url);
            }

            // Convert to signed URLs
            return Results.CreatedAtRoute("GetPostById", 
                new { id = post.Id }, 
                GenerateSignedUrlsForPost(post, imageStorage));
        }
        else
        {
            // JSON request
            var request = await context.Request.ReadAsJsonAsync<CreatePostRequest>();
            var post = await repository.CreateAsync(request);
            return Results.CreatedAtRoute("GetPostById", new { id = post.Id }, post);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating post");
        return Results.Problem("Failed to create post");
    }
}
```

### Pattern 2: Assign Tags to Content

**Flow:**
1. Client sends array of tag IDs
2. Validate all tag IDs exist
3. Clear existing post-tag relationships
4. Add new post-tag relationships
5. Return post with updated tags

**Code:**
```csharp
// SimpleBlog.Blog.Services/EfPostRepository.cs

public async Task<Post?> AssignTagsAsync(Guid postId, List<Guid> tagIds)
{
    var entity = await context.Posts
        .Include(p => p.PostTags)
            .ThenInclude(pt => pt.Tag)
        .FirstOrDefaultAsync(p => p.Id == postId);
    
    if (entity is null)
        return null;

    // Remove old tags
    context.PostTags.RemoveRange(entity.PostTags);

    // Add new tags
    foreach (var tagId in tagIds)
    {
        entity.PostTags.Add(new PostTagEntity
        {
            PostId = postId,
            TagId = tagId
        });
    }

    await context.SaveChangesAsync();
    return MapToModel(entity);
}
```

### Pattern 3: Slug Generation

**Automatic slug generation from tag name:**
```csharp
// SimpleBlog.Blog.Services/EfTagRepository.cs

private static string GenerateSlug(string name)
{
    // "ASP.NET Core" → "asp-net-core"
    var slug = name.ToLowerInvariant();
    slug = RemoveDiacritics(slug);
    slug = SlugRegex().Replace(slug, "-");
    slug = MultipleHyphensRegex().Replace(slug, "-");
    return slug.Trim('-');
}
```

---

## 🐛 Debugging Tips

### Issue: Images Not Uploading

**Symptoms:** 415 Unsupported Media Type error

**Debug:**
1. Check FormData not converted to JSON
2. Verify Content-Type NOT set manually (browser sets it)
3. Ensure files appended as 'images' key

**Fix:**
```typescript
// ❌ WRONG: Setting Content-Type manually
const headers = { 'Content-Type': 'application/json' };

// ✅ CORRECT: Let browser set multipart boundary
const formData = new FormData();
// Don't set Content-Type
```

### Issue: Slug Not Generating

**Symptoms:** Tag created but slug is wrong/empty

**Debug:**
1. Check Name field not empty
2. Verify slug generation regex
3. Check for duplicate slug conflict

**Fix:**
```csharp
// Ensure name is provided
var slug = GenerateSlug(request.Name ?? "untitled");
```

### Issue: Images Not Persisting

**Symptoms:** Images upload but disappear on refresh

**Debug:**
1. Check Cloudinary credentials valid
2. Verify signed URL generation working
3. Check URL expiration (60 minutes)

**Fix:**
```csharp
// Ensure GenerateSignedUrlsForPost called before returning
var signedPost = GenerateSignedUrlsForPost(post, imageStorage);
return Results.Ok(signedPost);
```

---

## 📊 Performance Considerations

### Image Upload Performance

**Optimization:**
- Client-side validation before upload (size, type)
- Parallel uploads (multiple files at once)
- Compression before upload (optional)
- Progress tracking for large files

```typescript
// Parallel upload optimization
const uploadPromises = files.map(file =>
  imageStorage.upload(file, 'posts')
);
const urls = await Promise.all(uploadPromises);
```

### Tag Query Performance

**Optimization:**
- Eager load tags with posts: `.Include(p => p.PostTags).ThenInclude(pt => pt.Tag)`
- Avoid N+1 queries
- Cache frequently used tags

```csharp
// ✅ GOOD: Single query with tags loaded
var post = await context.Posts
    .Include(p => p.PostTags)
        .ThenInclude(pt => pt.Tag)
    .FirstOrDefaultAsync(p => p.Id == id);

// ❌ BAD: Separate query per tag
var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
var tags = await context.PostTags.Where(pt => pt.PostId == id).ToListAsync();
```

---

## 🔒 Security Checklist

- [ ] Admin role verification for uploads
- [ ] File type validation (JPEG, PNG, GIF, WebP only)
- [ ] File size limit enforced (10MB max)
- [ ] Images stored privately on Cloudinary
- [ ] Signed URLs with expiration (60 min)
- [ ] Tag assignment restricted to admins
- [ ] SQL injection protection (EF Core parameterized)
- [ ] CORS properly configured
- [ ] HTTPS enforced in production

---

## 📚 Related Resources

- [Cloudinary Integration](./cloudinary-integration.md) - Storage details
- [Architecture Overview](./architecture-overview.md) - System design
- [Entity Framework Migrations](./entity-framework-migrations.md) - Database migrations

---

## ✅ Implementation Checklist

When implementing or extending these features:

- [ ] Understand multipart/form-data handling
- [ ] Implement file validation (size, type)
- [ ] Test image upload flow end-to-end
- [ ] Verify signed URL generation
- [ ] Create tag seed data
- [ ] Test tag assignment
- [ ] Verify permissions/authorization
- [ ] Load test with multiple large files
- [ ] Document new endpoints
- [ ] Update API tests

