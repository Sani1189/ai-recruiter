import { execSync } from 'child_process';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const projectRoot = path.join(__dirname, '..');
const csharpRoot = path.join(projectRoot, 'ai', 'CSharpSolutions');

try {
  console.log('[v0] Building C# project...');
  const buildCmd = `cd "${csharpRoot}" && dotnet build Recruiter/WebApi/WebApi.csproj`;
  const output = execSync(buildCmd, { encoding: 'utf-8', stdio: 'pipe' });
  console.log(output);
  console.log('[v0] Build completed successfully!');
} catch (error) {
  console.error('[v0] Build failed:');
  console.error(error.stdout || error.message);
  process.exit(1);
}
