SELECT sid, serial#, username, blocking_session, event
FROM v$session WHERE blocking_session IS NOT NULL;