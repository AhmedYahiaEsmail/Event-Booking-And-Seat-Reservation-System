import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../api/client';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';
import { ErrorAlert } from '../components/ui/Alert';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage(null);
    setErrorList(null);
    setIsSubmitting(true);

    try {
      await login(email, password);
      const redirectTo = location.state?.from?.pathname ?? '/';
      navigate(redirectTo, { replace: true });
    } catch (error) {
      if (error instanceof ApiError) {
        if (Array.isArray(error.errors) && error.errors.length > 0) {
          setErrorList(error.errors);
        } else {
          setErrorMessage(error.message);
        }
      } else {
        setErrorMessage('Something went wrong. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-73px)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
        <div className="h-1.5 w-full bg-primary" />

        <div className="p-7">
          <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Welcome back</p>
          <h1 className="mt-1 font-display text-xl font-semibold text-ink">Log in</h1>

          <form onSubmit={handleSubmit} className="mt-6 flex flex-col gap-4">
            <Input
              label="Email"
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />

            <Input
              label="Password"
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />

            <ErrorAlert message={errorMessage} list={errorList} />

            <Button type="submit" disabled={isSubmitting} className="mt-1 w-full">
              {isSubmitting ? 'Logging in...' : 'Log in'}
            </Button>
          </form>

          <p className="mt-6 text-center text-sm text-ink-soft">
            Don&apos;t have an account?{' '}
            <Link to="/register" className="font-medium text-primary hover:underline">
              Register
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}