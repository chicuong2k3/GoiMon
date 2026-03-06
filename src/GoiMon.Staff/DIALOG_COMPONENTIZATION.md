# GoiMon.Client — Dialog Componentization Guide

This guide defines when and how to extract dialogs into reusable components.

## Goals

- Reduce duplicated Razor markup across pages
- Keep destructive-action UX consistent
- Make dialog behavior easier to review and maintain

## Decision Rules

1. If a dialog pattern appears in **2+ screens**, extract it to `Shared/Components`.
2. If a dialog contains feature-specific form logic (complex fields, tabs, API-specific draft state), keep it feature-scoped under `Features/{FeatureName}/Components`.
3. Page files should keep only:
   - open/close state
   - submit/cancel handlers
   - data binding and validation state

## Current Shared Dialog Components

- `Shared/Components/ConfirmDialog.razor`
  - For irreversible confirmation actions (delete, bulk delete)
  - Standardized title/description/footer/action states
- `Shared/Components/FormDialog.razor`
  - For standard create/edit dialogs with common header/body/footer action layout
  - Keeps page logic in handlers while centralizing dialog shell UI

## Usage Pattern

```razor
<ConfirmDialog Open="@_showDeleteConfirm"
               OpenChanged="@(v => _showDeleteConfirm = v)"
               Title="Xóa sản phẩm?"
               Description="Hành động này không thể hoàn tác."
               OnConfirm="ConfirmDeleteAsync" />
```

```razor
<ConfirmDialog Open="@_showBulkDeleteConfirm"
               OpenChanged="@(v => _showBulkDeleteConfirm = v)"
               Title="@("Xóa " + _selectedIds.Count + " sản phẩm?")"
               Description="Hành động này không thể hoàn tác."
               ConfirmLoading="@_bulkDeleting"
               ConfirmLoadingText="Đang xóa..."
               OnConfirm="HandleBulkDelete" />
```

```razor
<FormDialog Open="@_showCreate"
      OpenChanged="@(v => _showCreate = v)"
      Title="Thêm danh mục"
      Description="Nhập tên danh mục mới."
      SubmitText="Lưu"
      SubmitLoading="@_saving"
      SubmitDisabled="@(string.IsNullOrWhiteSpace(_formName))"
      OnSubmit="HandleCreate">
  <BbInput @bind-Value="_formName" Placeholder="Tên danh mục" />
</FormDialog>
```

## UI Consistency Requirements

- Use BlazorBlueprint primitives (`BbDialog`, `BbButton`, etc.) only.
- Keep spacing aligned with app scale:
  - container/body: `px-4 md:px-5`
  - dialog section rhythm: `py-3/4`
- Use semantic variants (`ButtonVariant.Destructive`, `ButtonVariant.Outline`) instead of custom visual styles.

## Migration Checklist

- [ ] Identify duplicated dialog patterns via `grep` on `<BbDialog`.
- [ ] Extract reusable dialog shell/component.
- [ ] Replace repeated page-level markup.
- [ ] Verify loading/disabled/error action states still work.
- [ ] Build client project and smoke-test open/close + confirm behavior.
