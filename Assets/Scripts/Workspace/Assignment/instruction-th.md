# Assignment 09: การเรียนรู้ Stack และ Queue สำหรับ Game Development

## 🎯 จุดประสงค์การเรียนรู้

- เรียนรู้การใช้งาน Stack และ Queue พื้นฐาน
- เข้าใจหลักการ LIFO (Last In First Out) และ FIFO (First In First Out)
- นำ Stack และ Queue มาใช้ในการแก้ปัญหาในเกม เช่น การจัดการ Action, Undo/Redo, Event Queue
- วิเคราะห์ประสิทธิภาพและการใช้งานที่เหมาะสมของ Stack และ Queue
- เขียน code ที่มีประสิทธิภาพในการจัดการข้อมูลแบบ linear ด้วย Stack และ Queue

## 📚 โครงสร้างของ Assignment

- **Lecture Methods (4 methods)** - การ implement ฝึกหัด Stack และ Queue พื้นฐาน พร้อมกันในห้องเรียน
- **Assignment Methods (2 methods)** - การประยุกต์ใช้ Stack และ Queue ในสถานการณ์เกม
- **Extra Methods (1 method)** - โจทย์เสริมสำหรับฝึกฝนเพิ่มเติม

---

## 🧠 ความรู้พื้นฐานที่ควรทราบ

### Stack คืออะไร?
Stack คือโครงสร้างข้อมูลแบบ LIFO (Last In First Out) โดยข้อมูลที่ใส่เข้าไปล่าสุดจะถูกนำออกมาก่อน

### Queue คืออะไร?
Queue คือโครงสร้างข้อมูลแบบ FIFO (First In First Out) โดยข้อมูลที่ใส่เข้าไปก่อนจะถูกนำออกมาก่อน

### ใช้ Stack และ Queue ทำอะไรบ้างในเกม?
- **Stack**: Undo/Redo system, Function call stack, Backtracking algorithms, Action history
- **Queue**: Event handling, Turn-based systems, BFS algorithms, Message queues, Loading sequences

### Operations พื้นฐาน
- **Stack**: Push (ใส่), Pop (นำออก), Peek (ดูบนสุด), Count (จำนวน)
- **Queue**: Enqueue (ใส่), Dequeue (นำออก), Peek (ดูหน้า), Count (จำนวน)

---

## รูปแบบการส่งคำตอบและตรวจผล

- เขียนคำตอบใน `StudentSolution.cs` โดยใช้ `return` ส่งค่าตามชนิดข้อมูลของแต่ละ method
- Test cases เรียก method และตรวจค่าที่คืนโดยตรง ไม่ต้องพิมพ์ผลลัพธ์ลง Console
- `LCT01` และ `LCT02` คืน named tuple โดยชื่อสมาชิกและลำดับต้องตรงกับ signature
- `LCT03` และ `LCT04` คืน `string[]` ที่มีเฉพาะชื่อ Action ตามลำดับที่นำออก
- `ASN01` คืน `string`; `ASN02` และ `EX01` คืน `bool` (`true` หรือ `false`)
- Method ในไฟล์เริ่มต้นยังมี `throw new NotImplementedException()` ให้นักศึกษาแทนที่ด้วยคำตอบ
- ข้อมูล `str` ในโจทย์เป็น string ที่ไม่ใช่ `null`; string ว่างเป็น input ที่ถูกต้อง

## Lecture Methods

### 1. LCT01_StackSyntax

**วัตถุประสงค์:** เรียนรู้การใช้งาน Stack พื้นฐาน

**คำอธิบายปัญหา:**
สาธิตการใช้งาน Stack โดยการ Push ข้อมูลเข้า Stack, ตรวจสอบจำนวน, Pop ข้อมูลออก, และ Peek ข้อมูลบนสุด

ในเกม Stack มักใช้ในการ:
- จัดการ Action history สำหรับ Undo/Redo
- จัดการ Function call ใน scripting systems
- จัดการ State ใน game flow

**Method Signature:**
```csharp
public (int Count, int Popped, int Peek, int CountAfterPeek) LCT01_StackSyntax()
```

**Logic ที่ต้อง implement:**
1. สร้าง Stack<int>
2. Push 1, 2, 3 ตามลำดับ
3. เก็บ Count ก่อน Pop ในสมาชิก Count
4. Pop และเก็บค่าที่นำออกในสมาชิก Popped
5. Peek และเก็บค่าบนสุดในสมาชิก Peek แล้วเก็บ Count หลัง Peek ในสมาชิก CountAfterPeek
6. คืน tuple ตามลำดับ `(Count, Popped, Peek, CountAfterPeek)`

**Test Cases:**
1. **Expected Return Value:**
   ```
   (Count: 3, Popped: 3, Peek: 2, CountAfterPeek: 2)
   ```

### 2. LCT02_QueueSyntax

**วัตถุประสงค์:** เรียนรู้การใช้งาน Queue พื้นฐาน

**คำอธิบายปัญหา:**
สาธิตการใช้งาน Queue โดยการ Enqueue ข้อมูลเข้า Queue, ตรวจสอบจำนวน, Dequeue ข้อมูลออก, และ Peek ข้อมูลหน้า

ในเกม Queue มักใช้ในการ:
- จัดการ Event queue ใน game loop
- จัดการ Turn-based combat systems
- จัดการ Loading sequences
- จัดการ Message passing ระหว่าง systems

**Method Signature:**
```csharp
public (int Count, int Dequeued, int Peek, int CountAfterPeek) LCT02_QueueSyntax()
```

**Logic ที่ต้อง implement:**
1. สร้าง Queue<int>
2. Enqueue 1, 2, 3 ตามลำดับ
3. เก็บ Count ก่อน Dequeue ในสมาชิก Count
4. Dequeue และเก็บค่าที่นำออกในสมาชิก Dequeued
5. Peek และเก็บค่าหน้าในสมาชิก Peek แล้วเก็บ Count หลัง Peek ในสมาชิก CountAfterPeek
6. คืน tuple ตามลำดับ `(Count, Dequeued, Peek, CountAfterPeek)`

**Test Cases:**
1. **Expected Return Value:**
   ```
   (Count: 3, Dequeued: 1, Peek: 2, CountAfterPeek: 2)
   ```

### 3. LCT03_ActionStack

**วัตถุประสงค์:** เรียนรู้การใช้งาน Stack กับ Object

**คำอธิบายปัญหา:**
สาธิตการใช้งาน Stack ในการจัดการ Action objects โดย Push actions เข้า Stack แล้ว Pop ออกมา execute ตามลำดับ LIFO

ในเกมสามารถใช้ในการ:
- จัดการ Action stack สำหรับ command patterns
- Implement Undo system โดย pop actions ออกมาคืนค่า
- จัดการ State transitions ใน game flow

**Method Signature:**
```csharp
public string[] LCT03_ActionStack()
```

**Logic ที่ต้อง implement:**
1. ใช้ Action class ที่มีฟิลด์ Name ซึ่งเตรียมไว้ใน StudentSolution
2. ใช้ Action objects 3 ตัวที่เตรียมไว้ ชื่อ "Action 1", "Action 2", "Action 3"
3. Push เข้า Stack ตามลำดับ
4. Pop จน Stack ว่าง แล้วเก็บค่า Name ตามลำดับที่นำออก
5. คืนชื่อทั้งหมดเป็น string[] โดยไม่เติมข้อความ Executing

**Test Cases:**
1. **Expected Return Value:**
   ```
   new[] { "Action 3", "Action 2", "Action 1" }
   ```

### 4. LCT04_ActionQueue

**วัตถุประสงค์:** เรียนรู้การใช้งาน Queue กับ Object

**คำอธิบายปัญหา:**
สาธิตการใช้งาน Queue ในการจัดการ Action objects โดย Enqueue actions เข้า Queue แล้ว Dequeue ออกมา execute ตามลำดับ FIFO

ในเกมสามารถใช้ในการ:
- จัดการ Event queue ใน real-time systems
- Implement Turn-based action systems
- จัดการ Animation sequences
- จัดการ Network message queues

**Method Signature:**
```csharp
public string[] LCT04_ActionQueue()
```

**Logic ที่ต้อง implement:**
1. ใช้ Action class เดียวกับ LCT03
2. ใช้ Action objects 3 ตัวที่เตรียมไว้ ชื่อ "Action 1", "Action 2", "Action 3"
3. Enqueue เข้า Queue ตามลำดับ
4. Dequeue จน Queue ว่าง แล้วเก็บค่า Name ตามลำดับที่นำออก
5. คืนชื่อทั้งหมดเป็น string[] โดยไม่เติมข้อความ Executing

**Test Cases:**
1. **Expected Return Value:**
   ```
   new[] { "Action 1", "Action 2", "Action 3" }
   ```

---

## Assignment Methods

### ASN01_ReverseString

**วัตถุประสงค์:** ใช้ Stack ในการ Reverse String

**คำอธิบายปัญหา:**
เขียน method ที่รับ string และคืน string ที่กลับลำดับตัวอักษรโดยใช้ Stack โดยต้องคงอักขระทุกตัวรวมถึงช่องว่าง

ในเกมสามารถใช้ในการ:
- สร้าง Effect การพลิกคำหรือข้อความ
- Implement Text animation ที่ reverse
- จัดการ String manipulation ใน puzzle games
- สร้าง Password หรือ code ที่ต้อง reverse เพื่อใช้งาน

**Method Signature:**
```csharp
public string ASN01_ReverseString(string str)
```

**Logic ที่ต้อง implement:**
1. สร้าง Stack<char>
2. Push แต่ละ character ของ str เข้า Stack
3. Pop ออกมาทีละตัวและ concat เป็น string ใหม่
4. คืน string ที่ reverse แล้วด้วย return โดยไม่ใช้ Trim()

**Test Cases:**
1. **Input:** str = "hello"
   **Expected Return Value:**
   ```
   "olleh"
   ```

2. **Input:** str = "world"
   **Expected Return Value:**
   ```
   "dlrow"
   ```

3. **Input:** str = "a"
   **Expected Return Value:**
   ```
   "a"
   ```

4. **Input:** str = ""
   **Expected Return Value:**
   ```
   ""
   ```

5. **Input:** str = "12345"
   **Expected Return Value:**
   ```
   "54321"
   ```

### ASN02_StackPalindrome

**วัตถุประสงค์:** ใช้ Stack ในการตรวจสอบ Palindrome

**คำอธิบายปัญหา:**
เขียนโปรแกรมที่รับ String และตรวจสอบว่าเป็น Palindrome หรือไม่โดยใช้ Stack

ในเกมสามารถใช้ในการ:
- สร้าง Puzzle ที่ต้องหาคำที่เป็น palindrome
- ตรวจสอบ Magic words หรือ incantations
- Implement Word games ที่ให้ bonus สำหรับ palindrome
- ตรวจสอบ Symmetry ใน game patterns

**Method Signature:**
```csharp
public bool ASN02_StackPalindrome(string str)
```

**Logic ที่ต้อง implement:**
1. ใช้ logic เดียวกับ ASN01 เพื่อสร้าง reversed string
2. เปรียบเทียบ str กับ reversed string
3. คืน true ถ้าเหมือนกัน หรือ false ถ้าไม่เหมือนกัน
4. เปรียบเทียบตัวอักษรตามจริง โดยแยกตัวพิมพ์ใหญ่/เล็กและไม่ตัดช่องว่าง; string ว่างเป็น palindrome

**Test Cases:**
1. **Input:** str = "radar"
   **Expected Return Value:**
   ```
   true
   ```

2. **Input:** str = "hello"
   **Expected Return Value:**
   ```
   false
   ```

3. **Input:** str = "a"
   **Expected Return Value:**
   ```
   true
   ```

4. **Input:** str = ""
   **Expected Return Value:**
   ```
   true
   ```

5. **Input:** str = "aba"
   **Expected Return Value:**
   ```
   true
   ```

6. **Input:** str = "abcba"
   **Expected Return Value:**
   ```
   true
   ```

7. **Input:** str = "ab"
   **Expected Return Value:**
   ```
   false
   ```

8. **Input:** str = "aa"
   **Expected Return Value:**
   ```
   true
   ```

---

## Extra Methods

### EX01_ParenthesesChecker

**วัตถุประสงค์:** ใช้ Stack ในการตรวจสอบ Balanced Parentheses

**คำอธิบายปัญหา:**
เขียนโปรแกรมที่รับ String ที่ประกอบด้วย parentheses ต่างๆ และตรวจสอบว่ามีการเปิด-ปิด ที่สมดุลหรือไม่โดยใช้ Stack

ในเกมสามารถใช้ในการ:
- ตรวจสอบ Syntax ใน custom scripting languages
- Implement Code validation ใน programming games
- ตรวจสอบ Bracket matching ใน puzzle games
- Validate mathematical expressions หรือ formulas

**Method Signature:**
```csharp
public bool EX01_ParenthesesChecker(string str)
```

**Logic ที่ต้อง implement:**
1. สร้าง Stack<char>
2. วนลูปแต่ละ character ใน str
3. ถ้าเป็น opening bracket (, [, { ให้ Push เข้า Stack
4. ถ้าเป็น closing bracket ), ], } แต่ Stack ว่าง ให้คืน false ทันที มิฉะนั้น Pop และตรวจคู่; ถ้าคู่ไม่ตรงกันให้คืน false
5. เมื่อวนลูปครบ คืน true ถ้า Stack ว่าง มิฉะนั้นคืน false
6. Input ในโจทย์มีเฉพาะวงเล็บ (), [], {} หรือเป็น string ว่าง

**Test Cases:**
1. **Input:** str = "()"
   **Expected Return Value:**
   ```
   true
   ```

2. **Input:** str = "([])"
   **Expected Return Value:**
   ```
   true
   ```

3. **Input:** str = "{[()]}"
   **Expected Return Value:**
   ```
   true
   ```

4. **Input:** str = ""
   **Expected Return Value:**
   ```
   true
   ```

5. **Input:** str = "("
   **Expected Return Value:**
   ```
   false
   ```

6. **Input:** str = ")"
   **Expected Return Value:**
   ```
   false
   ```

7. **Input:** str = "(]"
   **Expected Return Value:**
   ```
   false
   ```

8. **Input:** str = "([)]"
   **Expected Return Value:**
   ```
   false
   ```

9. **Input:** str = "(("
   **Expected Return Value:**
   ```
   false
   ```

10. **Input:** str = "))"
    **Expected Return Value:**
    ```
    false
    ```

---
