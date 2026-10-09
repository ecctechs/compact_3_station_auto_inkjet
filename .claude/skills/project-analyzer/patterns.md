# Pattern สำหรับค้นหา แยกตาม stack

ใช้กับ Glob / Grep ตอนทำขั้นที่ 1–6 ของ SKILL.md
ทุก pattern ต้องข้าม `node_modules`, `bin`, `obj`, `dist`, `build`, `.git` เสมอ

## C# / .NET

| หาอะไร | Glob / Grep |
|---|---|
| โปรเจกต์ | `**/*.sln`, `**/*.csproj` |
| ชนิดโปรเจกต์ | `OutputType`, `UseWindowsForms`, `UseWPF`, `Microsoft.NET.Sdk.Web`, `TargetFramework` |
| entry point | `static void Main`, `Application.Run\(`, `WebApplication.CreateBuilder`, `app.Run\(` |
| DI / service | `builder.Services.Add`, `services.Add(Singleton|Scoped|Transient)` |
| endpoint | `\[Http(Get|Post|Put|Delete)`, `app.Map(Get|Post|Put|Delete)`, `\[Route\(` |
| event ของ UI | `\.Click \+=`, `_Click\(object`, `\.Tick \+=`, `TextChanged \+=` |
| เรียก HTTP ออก | `HttpClient`, `GetAsync\(`, `PostAsync\(`, `BaseAddress` |
| socket / อุปกรณ์ | `TcpClient`, `Socket\(`, `SerialPort`, `Modbus`, `NetworkStream` |
| DB | `DbContext`, `SqliteConnection`, `SqlConnection`, `NpgsqlConnection`, `ExecuteReader`, `CREATE TABLE` |
| config | `appsettings*.json`, `ConfigurationManager`, `\.config$`, `Environment.GetEnvironmentVariable` |
| background | `Task.Run\(`, `System.Threading.Timer`, `BackgroundService`, `Thread\(` |

Designer ของ WinForms (`*.Designer.cs`) ยาวแต่เป็นแค่การวาง control — อ่านเฉพาะตอนต้องรู้ว่าปุ่มไหนผูกกับ event อะไร

## Node.js backend

| หาอะไร | Glob / Grep |
|---|---|
| entry point | `package.json` → `main`, `scripts.start`; Grep `app.listen\(`, `createServer\(` |
| middleware | `app.use\(` (ลำดับสำคัญ) |
| route | `router\.(get|post|put|patch|delete)\(`, `app\.(get|post|put|patch|delete)\(` |
| controller / service | โฟลเดอร์ `controllers/`, `services/`; Grep `module.exports`, `export (default|const|function)` |
| ORM / DB | `sequelize.define`, `Model.init`, `mongoose.Schema`, `prisma`, `knex`, `typeorm`, `\.sync\(` |
| migration | `migrations/`, `prisma/migrations/`, `knexfile` |
| validation | `zod`, `joi`, `yup`, `express-validator` |
| env | `process.env\.` (รายงานแค่ชื่อ key) |
| test | `*.test.js`, `*.spec.js`, `node:test`, `jest`, `mocha`, `vitest` |

## เว็บหน้าบ้าน

| หาอะไร | Glob / Grep |
|---|---|
| framework | `package.json` deps: `react`, `vue`, `svelte`, `@angular/core`, `next`, `nuxt`, `vite` |
| จุด mount | `index.html`, `createRoot\(`, `createApp\(`, `\.mount\(` |
| route | `createBrowserRouter`, `<Route`, `createRouter\(`, `routes:`, โฟลเดอร์ `pages/` หรือ `app/` |
| state | `createStore`, `defineStore` (Pinia), `useReducer`, `redux`, `zustand`, `useContext` |
| เรียก API | `fetch\(`, `axios\.`, `baseURL`, `import.meta.env`, `/api/` |
| component หลัก | โฟลเดอร์ `components/`, `views/`, `layouts/` — นับว่าถูก import จากกี่ที่ |

## จับคู่ client ↔ backend

1. Grep path ที่ client เรียก เช่น `"/api/` หรือ `"/job` ในโค้ด client
2. Grep path เดียวกันในไฟล์ route ของ backend
3. ระวัง prefix ที่ถูกต่อทีหลัง (`app.use("/api", router)`, `BaseAddress`, `baseURL`) — ต้องรวม prefix ก่อนเทียบ
4. path ที่มีตัวแปร (`/job/:id`, `$"/job/{id}"`) ให้เทียบเฉพาะส่วนคงที่
