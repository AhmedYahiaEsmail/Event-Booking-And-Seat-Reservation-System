import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../api/client';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage(null);
    setErrorList(null);

    // Only client-side rule beyond HTML5 `required`: catch a typo'd confirmation
    // before ever calling the API. Full password-complexity rules are enforced
    // by the backend and surfaced via ApiError.errors below.
    if (password !== confirmPassword) {
      setErrorMessage('Passwords do not match.');
      return;
    }

    setIsSubmitting(true);

    try {
      await register(firstName, lastName, email, password);
      navigate('/', { replace: true });
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
    <div className="flex min-h-[calc(100vh-65px)] items-center justify-center px-4">
      <div className="w-full max-w-sm rounded-md border border-gray-200 p-6">
        <h1 className="mb-6 text-lg font-semibold text-gray-900">Register</h1>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <label htmlFor="firstName" className="text-sm font-medium text-gray-700">
              First name
            </label>
            <input
              id="firstName"
              type="text"
              value={firstName}
              onChange={(event) => setFirstName(event.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="lastName" className="text-sm font-medium text-gray-700">
              Last name
            </label>
            <input
              id="lastName"
              type="text"
              value={lastName}
              onChange={(event) => setLastName(event.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="email" className="text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="password" className="text-sm font-medium text-gray-700">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="confirmPassword" className="text-sm font-medium text-gray-700">
              Confirm password
            </label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          {errorList && (
            <ul className="list-inside list-disc text-sm text-red-600">
              {errorList.map((message) => (
                <li key={message}>{message}</li>
              ))}
            </ul>
          )}

          {errorMessage && <p className="text-sm text-red-600">{errorMessage}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? 'Registering...' : 'Register'}
          </button>
        </form>

        <p className="mt-4 text-sm text-gray-600">
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-gray-900 hover:underline">
            Log in
          </Link>
        </p>
      </div>
    </div>
  );
}
