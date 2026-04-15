# AGENTS.md

## Repository working mode
This repository uses a strict review-first workflow.
The assistant must analyze first, propose second, and wait for approval before making any code changes.

## Current project priority
At the current stage, focus on building a clean and tight project foundation only.
Do not jump into complex business features immediately.
Prefer small, safe, maintainable additions that prepare the codebase for future features.

## Hard rules
- Do not modify source code immediately.
- Do not apply patches unless the user explicitly approves.
- Do not create, rename, move, or delete files unless the user explicitly approves.
- Do not run destructive commands.
- Do not refactor unrelated code.
- Do not redesign the architecture unless the user explicitly asks for it.
- Preserve the existing project structure as much as possible.
- Prefer minimal and low-risk changes.
- If something is unclear, state assumptions explicitly.
- If there are multiple implementation options, compare them and recommend the safest one.

## Approval gate
Do not implement any code changes until the user clearly says one of the following:
- "hãy implement"
- "hãy sửa code"
- "hãy apply patch"
- "make the changes"
- "now implement"
- "apply it"

Until such approval is given, remain in analysis + proposal mode only.

## Required working order for every request
For each task, always respond in this order:

1. **Hiểu hiện trạng**
   - Tóm tắt phần code liên quan trong project
   - Giải thích luồng hiện tại đang hoạt động như thế nào
   - Xác định vị trí an toàn nhất để tích hợp thay đổi mới

2. **Phân tích tác động**
   - Liệt kê các file liên quan
   - Giải thích vai trò của từng file
   - Nêu file nào cần sửa và vì sao
   - Giải thích các file này kết nối với file nào khác

3. **Đề xuất hướng làm**
   - Mô tả luồng nghiệp vụ đề xuất bằng ngôn ngữ dễ hiểu
   - Mô tả luồng dữ liệu / request flow / dependency flow
   - Nêu các rủi ro, edge cases, và assumptions
   - Chỉ rõ required changes và optional improvements

4. **Draft để review**
   - Đưa pseudocode trước nếu cần
   - Đưa code mẫu để review trước
   - Đánh dấu rõ: illustrative only / not yet applied
   - Không được tự ý sửa file

5. **Dừng và chờ duyệt**
   - Kết thúc bằng việc chờ user review
   - Không chuyển sang implement nếu chưa có approval rõ ràng

## Output format
Use this response structure whenever possible:

1. Hiểu hiện trạng
2. Luồng hiện tại
3. File liên quan
4. Vai trò từng file
5. Điểm tích hợp an toàn nhất
6. Luồng đề xuất giữa các file
7. Required changes
8. Optional improvements
9. Pseudocode
10. Code mẫu để review
11. Rủi ro / giả định / edge cases
12. Chờ user duyệt

## Foundation-first policy
At this stage, prioritize only foundational work unless the user explicitly asks otherwise.
Preferred work types:
- clarify project structure
- define boundaries between Controller / Service / Repository / Hub
- create or refine interfaces
- define DTO / request / response models
- define validation entry points
- define response wrappers if needed
- improve dependency injection registration
- improve middleware integration points
- clarify configuration structure
- create base service abstractions where truly needed
- create clean SignalR flow foundations
- prepare simple auth/authorization extension points if already present

Do not prioritize these yet unless explicitly requested:
- advanced business features
- large refactors
- optimization
- caching
- background workers
- distributed messaging
- event-driven redesign
- CQRS/DDD migration
- advanced real-time synchronization
- broad architecture replacement

## .NET / MVC / Web API / SignalR specific rules
When relevant, always explain:
- Controller receives request from which endpoint or UI flow
- Controller calls which Service
- Service depends on which Repository / DbContext / abstraction
- DTO maps to which Entity / ViewModel / response model
- Validation should happen at which layer
- Transactions should live at which layer
- Middleware / filters / auth / configuration involved
- SignalR Hub receives event from where
- Hub delegates logic directly or through which Service
- Which methods broadcast to which clients / groups / users
- Which files define contracts and which files define implementation

## Code sample rules
Every code sample must follow these rules:

- Every code block must include short Vietnamese comments.
- Comments must explain specifically:
  - đoạn code này đang làm gì
  - nó phục vụ logic nghiệp vụ nào
  - nó kết nối tới file / class / service / repository / controller / hub / DTO nào
  - dữ liệu đi từ đâu đến đâu nếu có
- Avoid vague comments such as:
  - xử lý logic
  - gọi hàm
  - khởi tạo
  - map dữ liệu
- Important lines or blocks should be commented directly in the code.
- Comments must be short, clear, practical, and written in Vietnamese.
- Do not remove existing useful comments unless necessary.

## File relationship explanation rule
For every affected file, explain clearly:
- file này giữ vai trò gì trong project
- vì sao cần sửa file này
- file này liên kết với những file nào khác
- nó nằm ở đâu trong luồng nghiệp vụ tổng thể

After each code sample, also include a short note with:
- File currently shown
- Files this code interacts with
- Why this interaction is needed for the business flow

## Strict implementation behavior
- Do not generate too much code at once if the foundation is not ready.
- Build one safe layer at a time.
- Prefer interface-first and flow-first design.
- Match the current coding style already present in the repository.
- If the repository is inconsistent, recommend a safe local convention only for the affected area.
- Do not silently introduce new patterns not already used in the project unless clearly justified.

## First response behavior for a new task
When a new feature request arrives:
- First scan the related code.
- Explain current architecture and related flow.
- Identify missing foundational pieces.
- Recommend the smallest safe next step.
- Provide review-only draft code.
- Wait for approval.

## Default reminder sentence
When uncertain, follow this principle:
**Analyze first. Propose first. Draft first. Do not implement until approved.**
