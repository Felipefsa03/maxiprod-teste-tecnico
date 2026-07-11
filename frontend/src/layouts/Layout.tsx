import React from 'react';
import { Link, Outlet, useLocation } from 'react-router-dom';
import './Layout.css';

export const Layout: React.FC = () => {
  const location = useLocation();

  return (
    <div className="layout">
      <nav className="sidebar">
        <div className="sidebar-header">
          <h1 className="sidebar-title">Controle de Gastos</h1>
        </div>
        <ul className="nav-list">
          <li>
            <Link
              to="/"
              className={`nav-link ${location.pathname === '/' ? 'active' : ''}`}
            >
              Pessoas
            </Link>
          </li>
          <li>
            <Link
              to="/transactions"
              className={`nav-link ${location.pathname === '/transactions' ? 'active' : ''}`}
            >
              Transações
            </Link>
          </li>
          <li>
            <Link
              to="/reports"
              className={`nav-link ${location.pathname === '/reports' ? 'active' : ''}`}
            >
              Relatórios
            </Link>
          </li>
        </ul>
      </nav>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
};
