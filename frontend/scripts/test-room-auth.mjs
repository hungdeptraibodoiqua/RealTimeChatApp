// Integration proof for SEC-03 and SEC-04 on local TEST accounts only.
// PowerShell: $env:DEMO_TOKEN_B='...'; $env:DEMO_ROOM_A='...'; node ./scripts/test-room-auth.mjs
// Do not log or upload test tokens in screenshots.
import * as signalR from '@microsoft/signalr';
const token = process.env.DEMO_TOKEN_B;
const room = process.env.DEMO_ROOM_A;
if (!token || !room) {
  console.error('Set DEMO_TOKEN_B (non-member token) and DEMO_ROOM_A (test room GUID)');
  process.exit(2);
}
const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:3000/hub/chat', { accessTokenFactory: () => token })
  .configureLogging(signalR.LogLevel.Error)
  .build();
let failed = false;
try {
  await connection.start();
  for (const tc of [
    { id: 'SEC-03', method: 'JoinRoom', args: [room], expected: 'Not authorized to join this room' },
    { id: 'SEC-04', method: 'SendTyping', args: [room, true], expected: 'Not authorized to send events to this room' },
  ]) {
    try {
      await connection.invoke(tc.method, ...tc.args);
      console.error(`FAIL ${tc.id}: non-member was allowed to invoke ${tc.method}`);
      failed = true;
    } catch (error) {
      const msg = String(error?.message ?? '');
      if (msg.includes(tc.expected)) {
        console.log(`PASS ${tc.id}: ${tc.method} denied for non-member`);
      } else {
        console.error(`INCONCLUSIVE ${tc.id}: Unexpected error: ${msg}`);
        failed = true;
      }
    }
  }
} catch (error) {
  console.error(`INCONCLUSIVE: cannot connect to local Hub: ${String(error?.message ?? '')}`);
  failed = true;
} finally {
  try { await connection.stop(); } catch { /* no active connection */ }
}
process.exit(failed ? 1 : 0);
