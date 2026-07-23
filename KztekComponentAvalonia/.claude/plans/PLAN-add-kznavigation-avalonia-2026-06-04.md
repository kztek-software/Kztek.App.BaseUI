---
task: add-kznavigation-avalonia
created: 2026-06-04
updated: 2026-06-04
status: in-progress
workflow: WF-FEATURE
priority: P2
---

# PLAN: Bổ sung KzNavigation cho KztekComponentAvalonia

## Mô tả
Port KzNavigation vào KztekComponentAvalonia (.NET 8, Avalonia 11.2.7).
UserControl dạng AXAML + code-behind (giống KzIPTextbox),
gồm 4 Button điều hướng + ô nhập trang thủ công + nhãn tổng trang.

## Nguồn yêu cầu
- Yêu cầu: "Bổ sung cho KztekComponentAvalonia"
- Workflow: WF-FEATURE (scale nhỏ)
- Agent chain: Senior Developer (code) → Senior Developer (demo) → Tech Lead (review)

## Thiết kế
```
[◀◀] [◀]  [ 3 ] / 10  [▶] [▶▶]
```
- 4 nav Button: local ControlTheme (Navy → hover Orange → pressed Navy-hover)
- TextBox với Border focus ring (Orange khi focus)
- StyledProperties: CurrentPage (int), TotalPages (int), KzSize (Sm/Md/Lg)
- Event: PageChanged → PageChangedEventArgs.NewPage
- OnPropertyChanged: auto-clamp CurrentPage trong [1, TotalPages]

## Phases & Steps

| # | Bước | Agent | Status | Artifact |
|---|------|-------|--------|----------|
| 1.1 | Tạo Controls/KzNavigation.axaml | Senior Developer | ✅ | Controls/KzNavigation.axaml |
| 1.2 | Tạo Controls/KzNavigation.axaml.cs | Senior Developer | ✅ | Controls/KzNavigation.axaml.cs |
| 2.1 | Cập nhật InputsPage.axaml (demo section) | Senior Developer | ✅ | Pages/InputsPage.axaml |
| 3.1 | Build & verify | Tech Lead | ✅ | 0 errors |

## Lịch sử cập nhật
| Ngày | Cập nhật | Agent |
|------|----------|-------|
| 2026-06-04 | Plan tạo mới | task-planner |
