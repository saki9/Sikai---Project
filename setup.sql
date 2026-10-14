




SET DEFINE OFF;



BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Student (
      StudentID NUMBER PRIMARY KEY,
      FullName VARCHAR2(100) NOT NULL,
      Email VARCHAR2(100) NOT NULL UNIQUE,
      Password VARCHAR2(255) NOT NULL,
      SecurityQuestion VARCHAR2(200),
      SecurityAnswer VARCHAR2(255)
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Teacher (
      TeacherID NUMBER PRIMARY KEY,
      FullName VARCHAR2(100) NOT NULL,
      Email VARCHAR2(100) NOT NULL UNIQUE,
      Password VARCHAR2(255) NOT NULL,
      SecurityQuestion VARCHAR2(200),
      SecurityAnswer VARCHAR2(255)
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

-- Class/enrolment records used by both student and teacher workflows.
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE class_seq START WITH 1 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE enrollment_seq START WITH 1 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE SikaiClasses (
      ClassID NUMBER PRIMARY KEY,
      ClassCode VARCHAR2(50) NOT NULL UNIQUE,
      ClassName VARCHAR2(200) NOT NULL,
      TeacherID NUMBER NOT NULL,
      SubjectID NUMBER
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SikaiClasses ADD (SubjectID NUMBER)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE ClassEnrollments (
      EnrollmentID NUMBER PRIMARY KEY,
      ClassID NUMBER NOT NULL REFERENCES SikaiClasses(ClassID),
      StudentID NUMBER NOT NULL REFERENCES Student(StudentID),
      EnrolledAt DATE DEFAULT SYSDATE,
      CONSTRAINT uq_class_student UNIQUE (ClassID, StudentID)
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE SEQUENCE student_seq START WITH 1 INCREMENT BY 1';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE SEQUENCE teacher_seq START WITH 1 INCREMENT BY 1';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE Student ADD (SecurityQuestion VARCHAR2(200), SecurityAnswer VARCHAR2(255))';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE Teacher ADD (SecurityQuestion VARCHAR2(200), SecurityAnswer VARCHAR2(255))';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Subjects (
      SubjectID NUMBER PRIMARY KEY,
      Code VARCHAR2(30) NOT NULL UNIQUE,
      Name VARCHAR2(150) NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Notes (
      NoteID NUMBER PRIMARY KEY,
      SubjectID NUMBER NOT NULL REFERENCES Subjects(SubjectID),
      OwnerRole VARCHAR2(10) NOT NULL CHECK (OwnerRole IN (''STUDENT'',''TEACHER'')),
      OwnerID NUMBER NOT NULL,
      Title VARCHAR2(200) NOT NULL,
      Content CLOB,
      CreatedAt DATE DEFAULT SYSDATE NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Tasks (
      TaskID NUMBER PRIMARY KEY,
      SubjectID NUMBER NOT NULL REFERENCES Subjects(SubjectID),
      OwnerRole VARCHAR2(10) NOT NULL CHECK (OwnerRole IN (''STUDENT'',''TEACHER'')),
      OwnerID NUMBER NOT NULL,
      Title VARCHAR2(200) NOT NULL,
      Description VARCHAR2(1000),
      Status VARCHAR2(30) DEFAULT ''To Do'' NOT NULL,
      Priority VARCHAR2(20) DEFAULT ''Medium'' NOT NULL,
      TaskType VARCHAR2(50) DEFAULT ''Assignment'' NOT NULL,
      DueDate DATE NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Resources (
      ResourceID NUMBER PRIMARY KEY,
      SubjectID NUMBER NOT NULL REFERENCES Subjects(SubjectID),
      OwnerRole VARCHAR2(10) NOT NULL CHECK (OwnerRole IN (''STUDENT'',''TEACHER'')),
      OwnerID NUMBER NOT NULL,
      Title VARCHAR2(200) NOT NULL,
      Description VARCHAR2(1000),
      ResourceType VARCHAR2(30) NOT NULL,
      Url VARCHAR2(1000),
      UploadedAt DATE DEFAULT SYSDATE NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE StudyPlans (
      PlanID NUMBER PRIMARY KEY,
      SubjectID NUMBER NOT NULL REFERENCES Subjects(SubjectID),
      OwnerRole VARCHAR2(10) NOT NULL CHECK (OwnerRole IN (''STUDENT'',''TEACHER'')),
      OwnerID NUMBER NOT NULL,
      Description VARCHAR2(500) NOT NULL,
      PlanDate DATE NOT NULL,
      StartTime VARCHAR2(10) NOT NULL,
      EndTime VARCHAR2(10) NOT NULL,
      StudyHours NUMBER(6,2) DEFAULT 0 NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE subject_seq START WITH 100 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE notes_seq START WITH 100 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE tasks_seq START WITH 100 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE resources_seq START WITH 100 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE studyplan_seq START WITH 100 INCREMENT BY 1'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/

-- Test accounts. MERGE preserves IDs and class/enrolment links when this script is rerun.
MERGE INTO Teacher t
USING (SELECT 'teacher@sikai.test' AS Email FROM dual) seed
ON (LOWER(t.Email)=seed.Email)
WHEN MATCHED THEN UPDATE SET t.FullName='Test Teacher', t.Password='F+z5XRQe1E2oiJmjtJVySQ==:FDIlJ3goAT51tREapPzSOHsAL24MXjanMDBa5NY6IU8=', t.SecurityQuestion='What is your favourite colour?', t.SecurityAnswer='gIWkIL+T9oa4jP2v/omWRw==:K4otIwkSVobPQAAInsYuinKN6SJsupFeDt9gRuLITcE='
WHEN NOT MATCHED THEN INSERT (TeacherID, FullName, Email, Password, SecurityQuestion, SecurityAnswer)
VALUES (teacher_seq.NEXTVAL, 'Test Teacher', 'teacher@sikai.test', 'F+z5XRQe1E2oiJmjtJVySQ==:FDIlJ3goAT51tREapPzSOHsAL24MXjanMDBa5NY6IU8=', 'What is your favourite colour?', 'gIWkIL+T9oa4jP2v/omWRw==:K4otIwkSVobPQAAInsYuinKN6SJsupFeDt9gRuLITcE=');

MERGE INTO Student st
USING (SELECT 'student@sikai.test' AS Email FROM dual) seed
ON (LOWER(st.Email)=seed.Email)
WHEN MATCHED THEN UPDATE SET st.FullName='Test Student', st.Password='kV/tmMIspmuBKgijtIeWlg==:gFaXlQKjJob3LWMWh5445TM1mt9lsDZaKJ7QbSDwIK8=', st.SecurityQuestion='What is your favourite colour?', st.SecurityAnswer='BKX98QnBozID/vUnjWXLWg==:WzxF/PD+AYgEdBkPV9M0FeS+i3kuXL+gYVwACPAfj0s='
WHEN NOT MATCHED THEN INSERT (StudentID, FullName, Email, Password, SecurityQuestion, SecurityAnswer)
VALUES (student_seq.NEXTVAL, 'Test Student', 'student@sikai.test', 'kV/tmMIspmuBKgijtIeWlg==:gFaXlQKjJob3LWMWh5445TM1mt9lsDZaKJ7QbSDwIK8=', 'What is your favourite colour?', 'BKX98QnBozID/vUnjWXLWg==:WzxF/PD+AYgEdBkPV9M0FeS+i3kuXL+gYVwACPAfj0s=');

-- Subjects.
INSERT INTO Subjects(SubjectID,Code,Name) SELECT subject_seq.NEXTVAL,'DBMS','Database Management System' FROM dual WHERE NOT EXISTS (SELECT 1 FROM Subjects WHERE Code='DBMS');
INSERT INTO Subjects(SubjectID,Code,Name) SELECT subject_seq.NEXTVAL,'OS','Operating System' FROM dual WHERE NOT EXISTS (SELECT 1 FROM Subjects WHERE Code='OS');
INSERT INTO Subjects(SubjectID,Code,Name) SELECT subject_seq.NEXTVAL,'CN','Computer Network' FROM dual WHERE NOT EXISTS (SELECT 1 FROM Subjects WHERE Code='CN');
INSERT INTO Subjects(SubjectID,Code,Name) SELECT subject_seq.NEXTVAL,'NM','Numerical Methods' FROM dual WHERE NOT EXISTS (SELECT 1 FROM Subjects WHERE Code='NM');
INSERT INTO Subjects(SubjectID,Code,Name) SELECT subject_seq.NEXTVAL,'SAD','System Analysis and Design' FROM dual WHERE NOT EXISTS (SELECT 1 FROM Subjects WHERE Code='SAD');

-- Sample records are database seed data, not UI hardcoding.
INSERT INTO Notes(NoteID,SubjectID,OwnerRole,OwnerID,Title,Content,CreatedAt)
SELECT notes_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'ER Diagram Notes','Entity relationship diagram, attributes, cardinality and examples.',TO_DATE('2026-09-25','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='DBMS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Notes n WHERE n.OwnerRole='STUDENT' AND n.OwnerID=st.StudentID AND n.Title='ER Diagram Notes');

INSERT INTO Notes(NoteID,SubjectID,OwnerRole,OwnerID,Title,Content,CreatedAt)
SELECT notes_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'Process Scheduling','FCFS, SJF, Priority and Round Robin with examples.',TO_DATE('2026-09-21','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='OS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Notes n WHERE n.OwnerRole='STUDENT' AND n.OwnerID=st.StudentID AND n.Title='Process Scheduling');

INSERT INTO Notes(NoteID,SubjectID,OwnerRole,OwnerID,Title,Content,CreatedAt)
SELECT notes_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'OSI Model','Seven layers, functions, protocols and important points.',TO_DATE('2026-09-18','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='CN' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Notes n WHERE n.OwnerRole='STUDENT' AND n.OwnerID=st.StudentID AND n.Title='OSI Model');

INSERT INTO Notes(NoteID,SubjectID,OwnerRole,OwnerID,Title,Content,CreatedAt)
SELECT notes_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'Numerical Integration','Trapezoidal rule and Simpson rules with solved examples.',TO_DATE('2026-09-16','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='NM' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Notes n WHERE n.OwnerRole='STUDENT' AND n.OwnerID=st.StudentID AND n.Title='Numerical Integration');

INSERT INTO Tasks(TaskID,SubjectID,OwnerRole,OwnerID,Title,Description,Status,Priority,TaskType,DueDate)
SELECT tasks_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'DBMS Chapter 3 Questions','Solve past year questions on relational algebra.','To Do','High','Assignment',TO_DATE('2026-09-30','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='DBMS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Tasks t WHERE t.OwnerRole='STUDENT' AND t.OwnerID=st.StudentID AND t.Title='DBMS Chapter 3 Questions');

INSERT INTO Tasks(TaskID,SubjectID,OwnerRole,OwnerID,Title,Description,Status,Priority,TaskType,DueDate)
SELECT tasks_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'OS Memory Management Notes','Revise paging, segmentation and page replacement.','In Progress','Medium','Assignment',TO_DATE('2026-10-01','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='OS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Tasks t WHERE t.OwnerRole='STUDENT' AND t.OwnerID=st.StudentID AND t.Title='OS Memory Management Notes');

INSERT INTO Tasks(TaskID,SubjectID,OwnerRole,OwnerID,Title,Description,Status,Priority,TaskType,DueDate)
SELECT tasks_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'Numerical Methods Tutorial 1','Complete tutorial questions and submit.','Completed','Medium','Assignment',TO_DATE('2026-09-24','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='NM' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Tasks t WHERE t.OwnerRole='STUDENT' AND t.OwnerID=st.StudentID AND t.Title='Numerical Methods Tutorial 1');

INSERT INTO Tasks(TaskID,SubjectID,OwnerRole,OwnerID,Title,Description,Status,Priority,TaskType,DueDate)
SELECT tasks_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'CN Write Short Notes','Write short notes on OSI model and TCP vs UDP.','To Do','Medium','Assignment',TO_DATE('2026-10-02','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='CN' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Tasks t WHERE t.OwnerRole='STUDENT' AND t.OwnerID=st.StudentID AND t.Title='CN Write Short Notes');

INSERT INTO Resources(ResourceID,SubjectID,OwnerRole,OwnerID,Title,Description,ResourceType,Url,UploadedAt)
SELECT resources_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'DBMS Unit 3','Relational Algebra','PDF','https://example.com/sikai/dbms-unit-3',TO_DATE('2026-09-25','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='DBMS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Resources r WHERE r.OwnerRole='STUDENT' AND r.OwnerID=st.StudentID AND r.Title='DBMS Unit 3');

INSERT INTO Resources(ResourceID,SubjectID,OwnerRole,OwnerID,Title,Description,ResourceType,Url,UploadedAt)
SELECT resources_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'OS Unit 4','File Systems','PDF','https://example.com/sikai/os-unit-4',TO_DATE('2026-09-21','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='OS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Resources r WHERE r.OwnerRole='STUDENT' AND r.OwnerID=st.StudentID AND r.Title='OS Unit 4');

INSERT INTO Resources(ResourceID,SubjectID,OwnerRole,OwnerID,Title,Description,ResourceType,Url,UploadedAt)
SELECT resources_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'OSI Model Slides','Seven layers and protocols','Slides','https://example.com/sikai/osi-slides',TO_DATE('2026-09-18','YYYY-MM-DD')
FROM Subjects s, Student st WHERE s.Code='CN' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM Resources r WHERE r.OwnerRole='STUDENT' AND r.OwnerID=st.StudentID AND r.Title='OSI Model Slides');

INSERT INTO StudyPlans(PlanID,SubjectID,OwnerRole,OwnerID,Description,PlanDate,StartTime,EndTime,StudyHours)
SELECT studyplan_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'Revise relational algebra',TO_DATE('2026-09-30','YYYY-MM-DD'),'09:00','10:30',1.5
FROM Subjects s, Student st WHERE s.Code='DBMS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM StudyPlans p WHERE p.OwnerRole='STUDENT' AND p.OwnerID=st.StudentID AND p.Description='Revise relational algebra');

INSERT INTO StudyPlans(PlanID,SubjectID,OwnerRole,OwnerID,Description,PlanDate,StartTime,EndTime,StudyHours)
SELECT studyplan_seq.NEXTVAL,s.SubjectID,'STUDENT',st.StudentID,'Memory management revision',TO_DATE('2026-10-01','YYYY-MM-DD'),'11:00','12:00',1
FROM Subjects s, Student st WHERE s.Code='OS' AND st.Email='student@sikai.test'
AND NOT EXISTS (SELECT 1 FROM StudyPlans p WHERE p.OwnerRole='STUDENT' AND p.OwnerID=st.StudentID AND p.Description='Memory management revision');

-- Seed a simple classroom network for end-to-end testing of the DFD flows.
INSERT INTO SikaiClasses (ClassID, ClassCode, ClassName, TeacherID, SubjectID)
SELECT class_seq.NEXTVAL, s.Code, s.Name, t.TeacherID, s.SubjectID
FROM Subjects s CROSS JOIN Teacher t
WHERE t.Email='teacher@sikai.test'
  AND NOT EXISTS (SELECT 1 FROM SikaiClasses c WHERE c.ClassCode=s.Code);

INSERT INTO ClassEnrollments (EnrollmentID, ClassID, StudentID, EnrolledAt)
SELECT enrollment_seq.NEXTVAL, c.ClassID, st.StudentID, SYSDATE
FROM SikaiClasses c CROSS JOIN Student st
WHERE st.Email='student@sikai.test'
  AND NOT EXISTS (SELECT 1 FROM ClassEnrollments e WHERE e.ClassID=c.ClassID AND e.StudentID=st.StudentID);



BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE message_seq START WITH 1 INCREMENT BY 1';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE notification_seq START WITH 1 INCREMENT BY 1';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE SEQUENCE submission_seq START WITH 1 INCREMENT BY 1';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Messages (
      MessageID NUMBER PRIMARY KEY,
      SenderRole VARCHAR2(10) NOT NULL, SenderID NUMBER NOT NULL,
      ReceiverRole VARCHAR2(10) NOT NULL, ReceiverID NUMBER NOT NULL,
      MessageBody VARCHAR2(4000) NOT NULL,
      SentAt DATE DEFAULT SYSDATE NOT NULL, IsRead CHAR(1) DEFAULT ''N'' NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Notifications (
      NotificationID NUMBER PRIMARY KEY,
      RecipientRole VARCHAR2(10) NOT NULL, RecipientID NUMBER NOT NULL,
      Title VARCHAR2(250) NOT NULL, Body VARCHAR2(1000),
      CreatedAt DATE DEFAULT SYSDATE NOT NULL, IsRead CHAR(1) DEFAULT ''N'' NOT NULL
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE AssignmentSubmissions (
      SubmissionID NUMBER PRIMARY KEY,
      TaskID NUMBER NOT NULL REFERENCES Tasks(TaskID),
      StudentID NUMBER NOT NULL REFERENCES Student(StudentID),
      SubmissionText VARCHAR2(4000) NOT NULL,
      SubmittedAt DATE DEFAULT SYSDATE NOT NULL,
      Status VARCHAR2(30) DEFAULT ''Pending'' NOT NULL,
      Feedback VARCHAR2(2000),
      CONSTRAINT uq_task_student_submission UNIQUE(TaskID, StudentID)
  )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

COMMIT;
