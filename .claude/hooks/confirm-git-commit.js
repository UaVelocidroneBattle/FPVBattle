// PreToolUse hook: Claude may not commit on its own. Any git commit needs the user's confirmation.
let input = '';
process.stdin.on('data', chunk => (input += chunk));
process.stdin.on('end', () => {
    const command = JSON.parse(input).tool_input?.command ?? '';

    if (!/\bgit\b(\s+-\S+(\s+\S+)?)*\s+commit\b/.test(command))
        return;

    console.log(JSON.stringify({
        hookSpecificOutput: {
            hookEventName: 'PreToolUse',
            permissionDecision: 'ask',
            permissionDecisionReason: 'Commit only when the user has asked for it. Confirm this git commit.'
        }
    }));
});
