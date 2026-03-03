# 🖼️ Dev Story: Image Upload Service Integration

**Status:** Done (implemented 2026-03-04)  
**Date Created:** 2026-03-04  
**Owner:** Mary (Business Analyst)  
**User:** Chicuong  
**Story Key:** 4-1-image-upload

---

## Story

As a **staff/admin**,  
I want to **upload product/combo images**,  
so that **catalog screens display rich visuals**.

---

## Scope

### In Scope
- HTTP upload endpoint for images
- Validation: multipart/form-data, image mime type, max size 10MB
- Cloud upload service abstraction + Cloudinary implementation
- Integration in product/combo create/update mutations

### Out of Scope
- Video/file upload types beyond images
- Client-side image editing

---

## Acceptance Criteria

- [x] **AC1**: Endpoint `/api/upload` accepts image multipart file
- [x] **AC2**: Rejects non-image files
- [x] **AC3**: Rejects files larger than 10MB
- [x] **AC4**: Returns uploaded URL in response payload
- [x] **AC5**: Product and combo mutations can use uploaded image URL flow

---

## Implementation Evidence

- Endpoint: `src/GoiMon.Api/Features/ImageUpload/ImageUploadEndpoints.cs`
- Service contract: `src/GoiMon.Api/Features/ImageUpload/Services/IImageUploadService.cs`
- Cloud provider implementation: `src/GoiMon.Api/Features/ImageUpload/Services/CloudinaryImageUploadService.cs`
- Integration points: `src/GoiMon.Api/Features/Products/ProductMutations.cs`, `src/GoiMon.Api/Features/Combos/ComboMutations.cs`
- Client features: `src/GoiMon.Client/Features/ImageUpload/`

---

## Notes

- Upload capability is a shared platform feature used by multiple catalog domains.
