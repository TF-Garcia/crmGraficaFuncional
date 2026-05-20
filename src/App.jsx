import { useMemo, useState } from 'react'
import {
  AlertTriangle,
  ArrowRight,
  BadgeCheck,
  BarChart3,
  Boxes,
  CalendarClock,
  CheckCircle2,
  ChevronRight,
  ClipboardList,
  CreditCard,
  FileText,
  Filter,
  Home,
  Layers,
  Lock,
  LogIn,
  MessageCircle,
  Package,
  PackageCheck,
  PackagePlus,
  Palette,
  Phone,
  PieChart,
  Printer,
  ReceiptText,
  Search,
  Settings,
  ShieldCheck,
  ShoppingBag,
  Truck,
  Upload,
  UserRound,
  Users,
  WalletCards,
} from 'lucide-react'
import { clients, company, inventory, orders, products, productionColumns, productionJobs, statuses } from './data/mockData'
import { calculateQuote } from './services/quoteService'
import { createPaymentIntent, paymentMethods } from './services/paymentService'
import { formatDate, formatMoney, pluralizeDays } from './utils/formatters'

const adminRoutes = [
  ['dashboard', 'Dashboard', BarChart3],
  ['pedidos', 'Pedidos', ClipboardList],
  ['clientes', 'Clientes', Users],
  ['produtos', 'Produtos', Package],
  ['estoque', 'Estoque', Boxes],
  ['producao', 'Produção', Layers],
  ['pagamentos', 'Pagamentos', CreditCard],
  ['entregas', 'Entregas', Truck],
  ['relatorios', 'Relatórios', PieChart],
  ['configuracoes', 'Configurações', Settings],
]

const clientRoutes = [
  ['dashboard', 'Dashboard', Home],
  ['pedidos', 'Pedidos', ShoppingBag],
  ['orcamentos', 'Orçamentos', FileText],
  ['perfil', 'Perfil', UserRound],
]

const routeLabels = {
  '/': 'Início',
  '/catalogo': 'Catálogo',
  '/orcamento': 'Orçamento',
  '/login': 'Login',
  '/cadastro': 'Cadastro',
  '/contato': 'Contato',
}

function getInitialRoute() {
  return window.location.pathname === '/' ? '/' : window.location.pathname
}

function navigate(path, setRoute) {
  window.history.pushState({}, '', path)
  setRoute(path)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function App() {
  const [route, setRoute] = useState(getInitialRoute)
  const [session, setSession] = useState(() => JSON.parse(localStorage.getItem('printflow_session') || 'null'))
  const [quoteProductId, setQuoteProductId] = useState('cartoes-visita')

  window.onpopstate = () => setRoute(getInitialRoute())

  const selectedProduct = products.find((product) => product.id === quoteProductId) ?? products[0]

  const goto = (path) => navigate(path, setRoute)
  const startQuote = (productId) => {
    setQuoteProductId(productId)
    goto('/orcamento')
  }

  const auth = {
    session,
    login(role) {
      const profile =
        role === 'admin'
          ? { id: 'admin-1', name: 'Admin PrintFlow', email: 'admin@printflowpro.com.br', role: 'admin' }
          : { id: 'c1', name: 'Studio Bella', email: 'contato@studiobella.com.br', role: 'client' }
      localStorage.setItem('printflow_session', JSON.stringify(profile))
      setSession(profile)
      goto(role === 'admin' ? '/admin/dashboard' : '/cliente/dashboard')
    },
    logout() {
      localStorage.removeItem('printflow_session')
      setSession(null)
      goto('/')
    },
  }

  let page
  if (route.startsWith('/admin')) {
    page = <AdminArea goto={goto} auth={auth} active={route.split('/')[2] || 'dashboard'} />
  } else if (route.startsWith('/cliente')) {
    page = <ClientArea goto={goto} auth={auth} active={route.split('/')[2] || 'dashboard'} />
  } else if (route.startsWith('/produto/')) {
    page = <ProductDetail goto={goto} productId={route.split('/')[2]} startQuote={startQuote} />
  } else if (route === '/catalogo') {
    page = <CatalogPage goto={goto} startQuote={startQuote} />
  } else if (route === '/orcamento') {
    page = <QuotePage product={selectedProduct} setProductId={setQuoteProductId} goto={goto} />
  } else if (route === '/login' || route === '/cadastro') {
    page = <AuthPage auth={auth} mode={route === '/cadastro' ? 'signup' : 'login'} />
  } else if (route === '/contato') {
    page = <ContactPage />
  } else {
    page = <LandingPage goto={goto} startQuote={startQuote} />
  }

  return (
    <>
      {!route.startsWith('/admin') && !route.startsWith('/cliente') && (
        <PublicHeader goto={goto} route={route} session={session} auth={auth} />
      )}
      {page}
    </>
  )
}

function PublicHeader({ goto, route, session, auth }) {
  return (
    <header className="public-header">
      <button className="brand-button" type="button" onClick={() => goto('/')}>
        <span className="brand-mark"><Printer size={20} /></span>
        <span>{company.name}</span>
      </button>
      <nav className="top-nav" aria-label="Navegação pública">
        {Object.entries(routeLabels).map(([path, label]) => (
          <button className={route === path ? 'active' : ''} key={path} type="button" onClick={() => goto(path)}>
            {label}
          </button>
        ))}
      </nav>
      <div className="header-actions">
        {session ? (
          <>
            <button className="ghost-button" type="button" onClick={() => goto(session.role === 'admin' ? '/admin/dashboard' : '/cliente/dashboard')}>
              <ShieldCheck size={17} /> Painel
            </button>
            <button className="icon-button" type="button" onClick={auth.logout} title="Sair">
              <Lock size={17} />
            </button>
          </>
        ) : (
          <button className="primary-button" type="button" onClick={() => goto('/login')}>
            <LogIn size={17} /> Entrar
          </button>
        )}
      </div>
    </header>
  )
}

function LandingPage({ goto, startQuote }) {
  return (
    <main>
      <section className="hero">
        <div className="hero-copy">
          <p className="eyebrow">Gráfica rápida, estoque e produção em uma única plataforma</p>
          <h1>{company.legalName}</h1>
          <p>
            Catálogo online, orçamento automático, pedido com upload de arte, pagamento preparado para gateway e CRM
            administrativo para acompanhar cada etapa até a retirada ou entrega.
          </p>
          <div className="hero-actions">
            <button className="primary-button xl" type="button" onClick={() => goto('/orcamento')}>
              Fazer orçamento <ArrowRight size={18} />
            </button>
            <a className="whatsapp-button" href={`https://wa.me/${company.whatsapp}`} target="_blank" rel="noreferrer">
              <MessageCircle size={18} /> WhatsApp
            </a>
          </div>
          <div className="trust-row">
            <span><BadgeCheck size={16} /> Aprovação de arte</span>
            <span><WalletCards size={16} /> Pix, cartão ou retirada</span>
            <span><CalendarClock size={16} /> Prazo estimado</span>
          </div>
        </div>
      </section>

      <section className="section">
        <SectionHeading eyebrow="Diferenciais" title="Tudo que a gráfica precisa para vender, produzir e acompanhar." />
        <div className="feature-grid">
          {[
            ['Catálogo comercial', 'Produtos com imagens, prazo base, variações, upload de arte e pagamento configurável.', Package],
            ['Orçamento automático', 'Regras simuladas por tamanho, quantidade, material, urgência e entrega.', ReceiptText],
            ['Produção Kanban', 'Fila de trabalho por arte, impressão, acabamento e pronto para retirada.', Layers],
            ['Gestão financeira', 'Pagamentos Pix, cartão, retirada, comprovantes e status manual.', CreditCard],
          ].map(([title, text, Icon]) => (
            <article className="feature-card" key={title}>
              <Icon size={24} />
              <h3>{title}</h3>
              <p>{text}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="section tinted">
        <SectionHeading eyebrow="Catálogo" title="Produtos e serviços mais pedidos." action={<button className="ghost-button" onClick={() => goto('/catalogo')}>Ver catálogo</button>} />
        <ProductGrid products={products.slice(0, 4)} startQuote={startQuote} goto={goto} />
      </section>

      <section className="section portfolio">
        <SectionHeading eyebrow="Portfólio" title="Materiais para balcão, empresas, eventos e campanhas." />
        <div className="portfolio-grid">
          {products.slice(0, 6).map((product) => (
            <button className="portfolio-item" key={product.id} type="button" onClick={() => goto(`/produto/${product.id}`)}>
              <img src={product.image} alt="" />
              <span>{product.name}</span>
            </button>
          ))}
        </div>
      </section>

      <ContactBand />
    </main>
  )
}

function CatalogPage({ goto, startQuote }) {
  const [category, setCategory] = useState('Todos')
  const [query, setQuery] = useState('')
  const categories = ['Todos', ...new Set(products.map((product) => product.category))]
  const filtered = products.filter((product) => {
    const matchesCategory = category === 'Todos' || product.category === category
    const matchesQuery = product.name.toLowerCase().includes(query.toLowerCase()) || product.description.toLowerCase().includes(query.toLowerCase())
    return matchesCategory && matchesQuery
  })

  return (
    <main className="page-shell">
      <SectionHeading eyebrow="Catálogo" title="Escolha um produto para ver detalhes ou iniciar orçamento." />
      <div className="catalog-toolbar">
        <label className="search-box">
          <Search size={18} />
          <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Buscar produto ou serviço" />
        </label>
        <div className="chip-row">
          {categories.map((item) => (
            <button className={item === category ? 'chip active' : 'chip'} key={item} type="button" onClick={() => setCategory(item)}>
              {item}
            </button>
          ))}
        </div>
      </div>
      <ProductGrid products={filtered} startQuote={startQuote} goto={goto} />
    </main>
  )
}

function ProductGrid({ products: items, startQuote, goto }) {
  return (
    <div className="product-grid">
      {items.map((product) => (
        <article className="product-card" key={product.id}>
          <img src={product.image} alt="" />
          <div>
            <span className="tag">{product.category}</span>
            <h3>{product.name}</h3>
            <p>{product.description}</p>
          </div>
          <div className="product-meta">
            <strong>{formatMoney(product.basePrice)}</strong>
            <span>{pluralizeDays(product.baseDeadline)}</span>
          </div>
          <div className="row-actions">
            <button className="ghost-button" type="button" onClick={() => goto(`/produto/${product.id}`)}>
              Detalhes
            </button>
            <button className="primary-button" type="button" onClick={() => startQuote(product.id)}>
              Orçar
            </button>
          </div>
        </article>
      ))}
    </div>
  )
}

function ProductDetail({ productId, startQuote, goto }) {
  const product = products.find((item) => item.id === productId) ?? products[0]
  return (
    <main className="page-shell">
      <button className="link-button" type="button" onClick={() => goto('/catalogo')}>Catálogo</button>
      <section className="detail-layout">
        <img className="detail-image" src={product.image} alt="" />
        <div className="detail-copy">
          <span className="tag">{product.category}</span>
          <h1>{product.name}</h1>
          <p>{product.description}</p>
          <div className="detail-list">
            <span>Preço base: <strong>{formatMoney(product.basePrice)}</strong></span>
            <span>Prazo base: <strong>{pluralizeDays(product.baseDeadline)}</strong></span>
            <span>Upload de arte: <strong>{product.allowUpload ? 'Permitido' : 'Não aplicável'}</strong></span>
            <span>Pagamento na retirada: <strong>{product.allowPickupPayment ? 'Permitido' : 'Exige antecipado'}</strong></span>
          </div>
          <button className="primary-button xl" type="button" onClick={() => startQuote(product.id)}>
            Iniciar orçamento <ChevronRight size={18} />
          </button>
        </div>
      </section>
    </main>
  )
}

function QuotePage({ product, setProductId, goto }) {
  const [config, setConfig] = useState({
    quantity: product.quantities[0],
    size: product.sizes[0].name,
    material: product.materials[0].name,
    printMode: product.printModes[0].name,
    finishing: product.finishings[0].name,
    urgency: 'normal',
    delivery: 'retirada',
    paymentMethod: 'pix',
    notes: '',
    fileName: '',
  })

  const currentProduct = products.find((item) => item.id === product.id) ?? products[0]
  const quote = calculateQuote(currentProduct, config)
  const allowedPayments = paymentMethods.filter((method) => method.id !== 'pickup' || currentProduct.allowPickupPayment)
  const simulatedOrder = { id: 'novo', amount: quote.total }
  const paymentIntent = createPaymentIntent(simulatedOrder, config.paymentMethod)

  function updateProduct(id) {
    const next = products.find((item) => item.id === id)
    setProductId(id)
    setConfig({
      ...config,
      quantity: next.quantities[0],
      size: next.sizes[0].name,
      material: next.materials[0].name,
      printMode: next.printModes[0].name,
      finishing: next.finishings[0].name,
      paymentMethod: next.allowPickupPayment ? config.paymentMethod : 'pix',
    })
  }

  return (
    <main className="page-shell">
      <SectionHeading eyebrow="Orçamento automático" title="Configure o pedido e veja valor, prazo e pagamento." />
      <section className="quote-layout">
        <form className="quote-form">
          <Select label="Produto" value={currentProduct.id} onChange={updateProduct} options={products.map((item) => [item.id, item.name])} />
          <Select label="Quantidade" value={config.quantity} onChange={(quantity) => setConfig({ ...config, quantity })} options={currentProduct.quantities.map((item) => [item, item.toLocaleString('pt-BR')])} />
          <Select label="Tamanho" value={config.size} onChange={(size) => setConfig({ ...config, size })} options={currentProduct.sizes.map((item) => [item.name, item.name])} />
          <Select label="Material/papel" value={config.material} onChange={(material) => setConfig({ ...config, material })} options={currentProduct.materials.map((item) => [item.name, item.name])} />
          <Select label="Cores" value={config.printMode} onChange={(printMode) => setConfig({ ...config, printMode })} options={currentProduct.printModes.map((item) => [item.name, item.name])} />
          <Select label="Acabamento" value={config.finishing} onChange={(finishing) => setConfig({ ...config, finishing })} options={currentProduct.finishings.map((item) => [item.name, item.name])} />
          <Select label="Urgência" value={config.urgency} onChange={(urgency) => setConfig({ ...config, urgency })} options={[['normal', 'Normal'], ['expressa', 'Expressa +25%'], ['urgente', 'Urgente +45%']]} />
          <Select label="Retirada ou entrega" value={config.delivery} onChange={(delivery) => setConfig({ ...config, delivery })} options={[['retirada', 'Retirada'], ['entrega', 'Entrega local']]} />
          <label>
            Upload da arte
            <input type="file" onChange={(event) => setConfig({ ...config, fileName: event.target.files?.[0]?.name || '' })} />
          </label>
          <label>
            Observações
            <textarea value={config.notes} onChange={(event) => setConfig({ ...config, notes: event.target.value })} placeholder="Medidas especiais, referências, acabamento técnico..." />
          </label>
        </form>
        <aside className="quote-summary">
          <span className="tag">Resumo</span>
          <h2>{formatMoney(quote.total)}</h2>
          <p>Prazo estimado: <strong>{pluralizeDays(quote.estimatedDays)}</strong></p>
          <dl>
            <div><dt>Subtotal</dt><dd>{formatMoney(quote.subtotal)}</dd></div>
            <div><dt>Urgência</dt><dd>{formatMoney(quote.urgencyFee)}</dd></div>
            <div><dt>Entrega</dt><dd>{formatMoney(quote.deliveryFee)}</dd></div>
          </dl>
          <div className="quote-details">
            {quote.details.map((detail) => <span key={detail}>{detail}</span>)}
            {config.fileName && <span>Arte: {config.fileName}</span>}
          </div>
          <Select label="Pagamento" value={config.paymentMethod} onChange={(paymentMethod) => setConfig({ ...config, paymentMethod })} options={allowedPayments.map((item) => [item.id, item.label])} />
          <p className="service-note">{paymentIntent.message}</p>
          {!currentProduct.allowPickupPayment && <p className="service-note warning">Este produto exige pagamento antecipado.</p>}
          <button className="primary-button xl" type="button" onClick={() => goto('/login')}>
            Finalizar pedido <PackageCheck size={18} />
          </button>
        </aside>
      </section>
    </main>
  )
}

function AuthPage({ auth, mode }) {
  return (
    <main className="auth-shell">
      <section className="auth-card">
        <span className="brand-mark"><ShieldCheck size={22} /></span>
        <h1>{mode === 'signup' ? 'Criar conta de cliente' : 'Entrar na plataforma'}</h1>
        <p>Login simulado para navegar pelo painel do cliente ou pelo painel administrativo.</p>
        <div className="auth-form">
          <label>Email<input defaultValue={mode === 'signup' ? 'novo@cliente.com.br' : 'contato@studiobella.com.br'} /></label>
          <label>Senha<input type="password" defaultValue="123456" /></label>
          {mode === 'signup' && <label>Telefone<input defaultValue="(11) 90000-0000" /></label>}
        </div>
        <button className="primary-button xl" type="button" onClick={() => auth.login('client')}>Entrar como cliente</button>
        <button className="ghost-button xl" type="button" onClick={() => auth.login('admin')}>Entrar como administrador</button>
      </section>
    </main>
  )
}

function ClientArea({ goto, auth, active }) {
  const client = clients[0]
  const clientOrders = orders.filter((order) => order.clientId === client.id)
  return (
    <WorkspaceShell title="Área do cliente" routes={clientRoutes} active={active} base="/cliente" goto={goto} auth={auth}>
      {active === 'pedidos' && <ClientOrders clientOrders={clientOrders} />}
      {active === 'orcamentos' && <SavedQuotes />}
      {active === 'perfil' && <ProfileCard client={client} />}
      {(!active || active === 'dashboard') && (
        <>
          <div className="stats-grid">
            <MetricCard label="Pedidos ativos" value={clientOrders.length} tone="blue" />
            <MetricCard label="Orçamentos salvos" value="3" tone="amber" />
            <MetricCard label="Total gasto" value={formatMoney(client.totalSpent)} tone="green" />
            <MetricCard label="Arte pendente" value="1" tone="red" />
          </div>
          <div className="panel-grid two">
            <Panel title="Acompanhamento">
              <ClientOrders clientOrders={clientOrders} compact />
            </Panel>
            <Panel title="Atendimento">
              <p className="muted">Reenvie arquivos, aprove a prova final ou fale com atendimento pelo WhatsApp.</p>
              <div className="stack-actions">
                <button className="primary-button"><Upload size={17} /> Reenviar arte</button>
                <button className="ghost-button"><CheckCircle2 size={17} /> Aprovar arte</button>
                <a className="ghost-button" href={`https://wa.me/${company.whatsapp}`} target="_blank" rel="noreferrer"><MessageCircle size={17} /> Atendimento</a>
              </div>
            </Panel>
          </div>
        </>
      )}
    </WorkspaceShell>
  )
}

function AdminArea({ goto, auth, active }) {
  return (
    <WorkspaceShell title="Admin CRM" routes={adminRoutes} active={active} base="/admin" goto={goto} auth={auth}>
      {active === 'pedidos' && <OrdersAdmin />}
      {active === 'clientes' && <ClientsAdmin />}
      {active === 'produtos' && <ProductsAdmin />}
      {active === 'estoque' && <InventoryAdmin />}
      {active === 'producao' && <ProductionAdmin />}
      {active === 'pagamentos' && <PaymentsAdmin />}
      {active === 'entregas' && <DeliveriesAdmin />}
      {active === 'relatorios' && <ReportsAdmin />}
      {active === 'configuracoes' && <SettingsAdmin />}
      {(!active || active === 'dashboard') && <DashboardAdmin />}
    </WorkspaceShell>
  )
}

function WorkspaceShell({ title, routes, active, base, goto, auth, children }) {
  return (
    <main className="workspace-shell">
      <aside className="sidebar">
        <button className="workspace-brand" type="button" onClick={() => goto('/')}>
          <span className="brand-mark"><Printer size={20} /></span>
          <span>{company.name}<small>Ambiente demonstrativo</small></span>
        </button>
        <nav className="side-nav">
          {routes.map(([id, label, Icon]) => (
            <button key={id} className={active === id ? 'active' : ''} type="button" onClick={() => goto(`${base}/${id}`)}>
              <Icon size={18} /> {label}
            </button>
          ))}
        </nav>
      </aside>
      <section className="workspace-main">
        <header className="workspace-topbar">
          <div>
            <p className="eyebrow">{title}</p>
            <h1>{routes.find(([id]) => id === active)?.[1] || 'Dashboard'}</h1>
          </div>
          <div className="topbar-actions">
            <label className="search-box small"><Search size={17} /><input placeholder="Buscar..." /></label>
            <button className="ghost-button" type="button" onClick={auth.logout}>Sair</button>
          </div>
        </header>
        {children}
      </section>
    </main>
  )
}

function DashboardAdmin() {
  const revenue = orders.reduce((sum, order) => sum + order.amount, 0)
  return (
    <>
      <div className="stats-grid">
        <MetricCard label="Pedidos de hoje" value="9" tone="blue" />
        <MetricCard label="Em produção" value={orders.filter((order) => order.status === 'Em produção').length} tone="green" />
        <MetricCard label="Orçamentos pendentes" value="14" tone="amber" />
        <MetricCard label="Faturamento do mês" value={formatMoney(revenue + 48720)} tone="green" />
        <MetricCard label="Pagamentos pendentes" value="5" tone="red" />
        <MetricCard label="Prazos próximos" value="4" tone="amber" />
      </div>
      <div className="panel-grid three">
        <Panel title="Produtos mais vendidos"><RankList items={['Cartões de visita', 'Panfletos A5', 'Adesivos', 'Banners']} /></Panel>
        <Panel title="Clientes recorrentes"><RankList items={clients.map((client) => client.name)} /></Panel>
        <Panel title="Alertas importantes">
          {inventory.filter((item) => item.available < item.minimum).map((item) => (
            <p className="alert-line" key={item.id}><AlertTriangle size={16} /> {item.name} abaixo do mínimo</p>
          ))}
        </Panel>
      </div>
      <Panel title="Status dos pedidos"><StatusBoard /></Panel>
    </>
  )
}

function OrdersAdmin() {
  return (
    <Panel title="Gestão de pedidos" action={<button className="ghost-button"><Filter size={16} /> Filtros</button>}>
      <DataTable
        columns={['Pedido', 'Cliente', 'Produto', 'Prazo', 'Pagamento', 'Status', 'Responsável']}
        rows={orders.map((order) => {
          const client = clients.find((item) => item.id === order.clientId)
          const product = products.find((item) => item.id === order.productId)
          return [(`#${order.id}`), client.name, product.name, formatDate(order.deadline), order.paymentStatus, order.status, order.owner]
        })}
      />
    </Panel>
  )
}

function ClientsAdmin() {
  return (
    <div className="panel-grid two">
      <Panel title="Clientes">
        <DataTable columns={['Nome', 'Email', 'Telefone', 'Total gasto', 'Status']} rows={clients.map((client) => [client.name, client.email, client.phone, formatMoney(client.totalSpent), client.status])} />
      </Panel>
      <Panel title="Ficha do cliente">
        <ProfileCard client={clients[0]} />
      </Panel>
    </div>
  )
}

function ProductsAdmin() {
  return (
    <Panel title="Catálogo e produtos" action={<button className="primary-button"><PackagePlus size={16} /> Novo produto</button>}>
      <DataTable
        columns={['Produto', 'Categoria', 'Preço base', 'Prazo', 'Retirada', 'Status']}
        rows={products.map((product) => [product.name, product.category, formatMoney(product.basePrice), pluralizeDays(product.baseDeadline), product.allowPickupPayment ? 'Permite' : 'Antecipado', product.active ? 'Ativo' : 'Inativo'])}
      />
    </Panel>
  )
}

function InventoryAdmin() {
  return (
    <Panel title="Estoque e armazém" action={<button className="primary-button"><PackagePlus size={16} /> Entrada</button>}>
      <DataTable
        columns={['Material', 'Categoria', 'Disponível', 'Mínimo', 'Fornecedor', 'Custo', 'Status']}
        rows={inventory.map((item) => [item.name, item.category, `${item.available} ${item.unit}`, `${item.minimum} ${item.unit}`, item.supplier, formatMoney(item.unitCost), item.available < item.minimum ? 'Estoque baixo' : 'OK'])}
      />
    </Panel>
  )
}

function ProductionAdmin() {
  return (
    <div className="kanban">
      {productionColumns.map((column) => (
        <section className="kanban-column" key={column.id}>
          <h3>{column.title}</h3>
          {productionJobs.filter((job) => job.column === column.id).map((job) => (
            <article className="job-card" key={job.id}>
              <strong>#{job.id} {job.title}</strong>
              <span>{job.client}</span>
              <small>{formatDate(job.deadline)} • {job.priority} • {job.owner}</small>
              <p>{job.notes}</p>
            </article>
          ))}
        </section>
      ))}
    </div>
  )
}

function PaymentsAdmin() {
  return (
    <Panel title="Pagamentos">
      <div className="stats-grid compact">
        <MetricCard label="Recebido" value={formatMoney(31620)} tone="green" />
        <MetricCard label="Em aberto" value={formatMoney(4180)} tone="red" />
        <MetricCard label="Pix/cartão" value="68%" tone="blue" />
      </div>
      <DataTable columns={['Pedido', 'Método', 'Status', 'Valor', 'Ação']} rows={orders.map((order) => [`#${order.id}`, order.paymentMethod, order.paymentStatus, formatMoney(order.amount), order.paymentStatus === 'Confirmado' ? 'Comprovante' : 'Confirmar manualmente'])} />
    </Panel>
  )
}

function DeliveriesAdmin() {
  return (
    <Panel title="Entregas e retiradas">
      <DataTable columns={['Pedido', 'Cliente', 'Tipo', 'Endereço/taxa', 'Status']} rows={orders.map((order) => {
        const client = clients.find((item) => item.id === order.clientId)
        return [`#${order.id}`, client.name, order.delivery, order.delivery === 'Entrega' ? `${client.address} • ${formatMoney(company.deliveryFee)}` : 'Balcão', order.status]
      })} />
    </Panel>
  )
}

function ReportsAdmin() {
  return (
    <div className="panel-grid three">
      <Panel title="Faturamento por período"><p className="big-number">{formatMoney(62900)}</p><p className="muted">Simulação mensal com base nos pedidos aprovados.</p></Panel>
      <Panel title="Pedidos por status"><StatusBoard compact /></Panel>
      <Panel title="Produção atrasada"><p className="big-number warning-text">3</p><p className="muted">Itens precisam de revisão de prazo.</p></Panel>
      <Panel title="Estoque baixo"><RankList items={inventory.filter((item) => item.available < item.minimum).map((item) => item.name)} /></Panel>
      <Panel title="Produtos mais vendidos"><RankList items={products.slice(0, 5).map((item) => item.name)} /></Panel>
      <Panel title="Pagamentos pendentes"><p className="big-number">{formatMoney(4180)}</p></Panel>
    </div>
  )
}

function SettingsAdmin() {
  return (
    <Panel title="Configurações">
      <div className="settings-grid">
        <label>Nome da empresa<input defaultValue={company.legalName} /></label>
        <label>Telefone<input defaultValue={company.phone} /></label>
        <label>Chave Pix<input defaultValue={company.pixKey} /></label>
        <label>Taxa de entrega<input defaultValue={formatMoney(company.deliveryFee)} /></label>
        <label>Métodos aceitos<select defaultValue="todos"><option value="todos">Pix, cartão e retirada</option></select></label>
        <label>Integração futura<select defaultValue="mercado-pago"><option value="mercado-pago">Mercado Pago</option><option value="asaas">Asaas</option></select></label>
      </div>
    </Panel>
  )
}

function ClientOrders({ clientOrders, compact = false }) {
  const rows = clientOrders.map((order) => {
    const product = products.find((item) => item.id === order.productId)
    return [`#${order.id}`, product.name, order.status, formatDate(order.deadline), formatMoney(order.amount), order.paymentStatus]
  })
  return <DataTable columns={compact ? ['Pedido', 'Produto', 'Status'] : ['Pedido', 'Produto', 'Status', 'Prazo', 'Valor', 'Pagamento']} rows={compact ? rows.map((row) => row.slice(0, 3)) : rows} />
}

function SavedQuotes() {
  return (
    <div className="quote-status-grid">
      {products.slice(0, 3).map((product, index) => (
        <article className="mini-card" key={product.id}>
          <span className={index === 0 ? 'status-pill warning' : 'status-pill success'}>{index === 0 ? 'Pendente' : 'Aprovável'}</span>
          <strong>{product.name}</strong>
          <small>{formatMoney(product.basePrice * (index + 2))} • {pluralizeDays(product.baseDeadline)}</small>
          <button className="ghost-button">Pagar pedido</button>
        </article>
      ))}
    </div>
  )
}

function ProfileCard({ client }) {
  return (
    <div className="profile-card">
      <strong>{client.name}</strong>
      <span>{client.email}</span>
      <span>{client.phone}</span>
      <span>{client.document}</span>
      <span>{client.address}</span>
      <p>{client.notes}</p>
    </div>
  )
}

function DataTable({ columns, rows }) {
  return (
    <div className="table-wrap">
      <table>
        <thead><tr>{columns.map((column) => <th key={column}>{column}</th>)}</tr></thead>
        <tbody>{rows.map((row, index) => <tr key={`${row[0]}-${index}`}>{row.map((cell, cellIndex) => <td key={`${cell}-${cellIndex}`}>{renderCell(cell)}</td>)}</tr>)}</tbody>
      </table>
    </div>
  )
}

function renderCell(cell) {
  const text = String(cell)
  if (statuses.includes(text) || ['Pendente', 'Confirmado', 'Na retirada', 'Estoque baixo', 'OK', 'Ativo', 'Inativo', 'Recorrente'].includes(text)) {
    return <span className={`status-pill ${getStatusTone(text)}`}>{text}</span>
  }
  return text
}

function getStatusTone(text) {
  if (['Confirmado', 'OK', 'Ativo', 'Finalizado', 'Pagamento confirmado'].includes(text)) return 'success'
  if (['Pendente', 'Na retirada', 'Aguardando pagamento', 'Aguardando envio da arte', 'Aguardando aprovação do cliente', 'Recorrente'].includes(text)) return 'warning'
  if (['Estoque baixo', 'Cancelado'].includes(text)) return 'danger'
  return 'info'
}

function StatusBoard({ compact = false }) {
  const shown = compact ? statuses.slice(0, 5) : statuses.slice(0, 9)
  return <div className="status-board">{shown.map((status, index) => <span key={status} className={`status-pill ${getStatusTone(status)}`}>{status} <b>{index + 1}</b></span>)}</div>
}

function RankList({ items }) {
  return <div className="rank-list">{items.map((item, index) => <p key={item}><span>{index + 1}. {item}</span><strong>{index === 0 ? 'Top' : `${12 - index * 2}`}</strong></p>)}</div>
}

function Panel({ title, action, children }) {
  return (
    <section className="panel">
      <div className="panel-heading"><h2>{title}</h2>{action}</div>
      {children}
    </section>
  )
}

function MetricCard({ label, value, tone }) {
  return (
    <article className={`metric-card ${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}

function Select({ label, value, onChange, options }) {
  return (
    <label>
      {label}
      <select value={value} onChange={(event) => onChange(event.target.value)}>
        {options.map(([optionValue, optionLabel]) => <option key={optionValue} value={optionValue}>{optionLabel}</option>)}
      </select>
    </label>
  )
}

function SectionHeading({ eyebrow, title, action }) {
  return (
    <div className="section-heading">
      <div>
        <p className="eyebrow">{eyebrow}</p>
        <h2>{title}</h2>
      </div>
      {action}
    </div>
  )
}

function ContactPage() {
  return (
    <main className="page-shell">
      <SectionHeading eyebrow="Contato" title="Atendimento no balcão, WhatsApp e produção local." />
      <ContactBand />
    </main>
  )
}

function ContactBand() {
  return (
    <section className="contact-band">
      <div>
        <h2>Pronto para produzir?</h2>
        <p>{company.address} • {company.hours}</p>
      </div>
      <a className="primary-button xl" href={`https://wa.me/${company.whatsapp}`} target="_blank" rel="noreferrer">
        <Phone size={18} /> {company.phone}
      </a>
    </section>
  )
}

export default App
