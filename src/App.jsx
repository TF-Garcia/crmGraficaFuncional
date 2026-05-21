import { useEffect, useMemo, useState } from 'react'
import { CardPayment, initMercadoPago } from '@mercadopago/sdk-react'
import {
  AlertTriangle,
  ArrowRight,
  BarChart3,
  Boxes,
  CheckCircle2,
  ClipboardList,
  CreditCard,
  Eye,
  EyeOff,
  FileText,
  Home,
  Layers,
  Lock,
  LogIn,
  Package,
  PackageCheck,
  PackagePlus,
  Phone,
  PieChart,
  Printer,
  ReceiptText,
  Search,
  Settings,
  ShieldCheck,
  ShoppingBag,
  Truck,
  UserRound,
  Users,
} from 'lucide-react'
import { api, clearApiToken, saveApiToken } from './services/apiClient'
import { formatDate, formatMoney, pluralizeDays } from './utils/formatters'

const company = {
  name: 'PrintFlow Pro',
  legalName: 'Vera Grafica Digital',
  phone: '(11) 98888-2026',
  whatsapp: '5511988882026',
  address: 'Rua das Artes, 420 - Centro',
  hours: 'Segunda a sexta, 8h as 18h',
}

const adminRoutes = [
  ['dashboard', 'Dashboard', BarChart3],
  ['pedidos', 'Pedidos', ClipboardList],
  ['clientes', 'Clientes', Users],
  ['produtos', 'Produtos', Package],
  ['estoque', 'Estoque', Boxes],
  ['producao', 'Producao', Layers],
  ['pagamentos', 'Pagamentos', CreditCard],
  ['entregas', 'Entregas', Truck],
  ['relatorios', 'Relatorios', PieChart],
  ['configuracoes', 'Configuracoes', Settings],
]

const clientRoutes = [
  ['dashboard', 'Dashboard', Home],
  ['pedidos', 'Pedidos', ShoppingBag],
  ['orcamentos', 'Orcamentos', FileText],
  ['perfil', 'Perfil', UserRound],
]

const routeLabels = {
  '/': 'Inicio',
  '/catalogo': 'Catalogo',
  '/orcamento': 'Orcamento',
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

function toRole(role) {
  return String(role || '').toLowerCase()
}

function toProductOption(options) {
  return options?.map((item) => [item.name, item.name]) ?? []
}

function App() {
  const [route, setRoute] = useState(getInitialRoute)
  const [session, setSession] = useState(() => JSON.parse(localStorage.getItem('printflow_session') || 'null'))
  const [products, setProducts] = useState([])
  const [quoteProductId, setQuoteProductId] = useState('')
  const [notice, setNotice] = useState('')

  useEffect(() => {
    window.onpopstate = () => setRoute(getInitialRoute())
    refreshProducts()
  }, [])

  const refreshProducts = () => {
    api.products()
      .then((items) => {
        setProducts(items)
        setQuoteProductId((current) => current || items[0]?.id || '')
      })
      .catch(() => setNotice('API indisponivel. Verifique se a API esta rodando.'))
  }

  const goto = (path) => navigate(path, setRoute)
  const selectedProduct = products.find((product) => product.id === quoteProductId) ?? products[0]

  const auth = {
    session,
    setLoggedUser(payload) {
      const profile = {
        id: payload.userId,
        name: payload.name,
        email: payload.email,
        phone: payload.phone,
        role: toRole(payload.role),
      }
      saveApiToken(payload.token)
      localStorage.setItem('printflow_session', JSON.stringify(profile))
      setSession(profile)
      goto(profile.role === 'admin' ? '/admin/dashboard' : '/cliente/dashboard')
    },
    logout() {
      clearApiToken()
      localStorage.removeItem('printflow_session')
      setSession(null)
      goto('/')
    },
  }

  const startQuote = (productId) => {
    setQuoteProductId(productId)
    goto('/orcamento')
  }

  let page
  if (route.startsWith('/admin')) {
    page = session?.role === 'admin'
      ? <AdminArea goto={goto} auth={auth} active={route.split('/')[2] || 'dashboard'} products={products} refreshProducts={refreshProducts} />
      : <AuthPage auth={auth} mode="login" />
  } else if (route.startsWith('/cliente')) {
    page = session
      ? <ClientArea goto={goto} auth={auth} active={route.split('/')[2] || 'dashboard'} products={products} startQuote={startQuote} />
      : <AuthPage auth={auth} mode="login" />
  } else if (route.startsWith('/produto/')) {
    page = <ProductDetail goto={goto} productId={route.split('/')[2]} products={products} startQuote={startQuote} />
  } else if (route === '/catalogo') {
    page = <CatalogPage goto={goto} products={products} startQuote={startQuote} />
  } else if (route === '/orcamento') {
    page = <QuotePage product={selectedProduct} products={products} setProductId={setQuoteProductId} goto={goto} session={session} />
  } else if (route === '/login' || route === '/cadastro') {
    page = <AuthPage auth={auth} mode={route === '/cadastro' ? 'signup' : 'login'} />
  } else if (route === '/esqueci-senha' || route === '/recuperar-senha') {
    page = <PasswordRecovery route={route} />
  } else if (route === '/contato') {
    page = <ContactPage />
  } else {
    page = <LandingPage goto={goto} products={products} startQuote={startQuote} />
  }

  return (
    <>
      {!route.startsWith('/admin') && !route.startsWith('/cliente') && (
        <PublicHeader goto={goto} route={route} session={session} auth={auth} />
      )}
      {notice && <div className="service-note warning">{notice}</div>}
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
      <nav className="top-nav" aria-label="Navegacao publica">
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

function LandingPage({ goto, products, startQuote }) {
  return (
    <main>
      <section className="hero">
        <div className="hero-copy">
          <p className="eyebrow">Grafica rapida, estoque e producao em uma unica plataforma</p>
          <h1>{company.legalName}</h1>
          <p>Catalogo online, orcamento automatico, pedidos reais, area do cliente e CRM administrativo conectado ao MySQL.</p>
          <div className="hero-actions">
            <button className="primary-button xl" type="button" onClick={() => goto('/orcamento')}>
              Fazer orcamento <ArrowRight size={18} />
            </button>
            <button className="ghost-button xl" type="button" onClick={() => goto('/catalogo')}>Ver catalogo</button>
          </div>
        </div>
      </section>
      <section className="section">
        <SectionHeading eyebrow="Operacao real" title="Cliente compra, admin acompanha e o banco guarda tudo." />
        <div className="feature-grid">
          {[
            ['Cadastro e login', 'Sessao real com JWT e dados por usuario.', ShieldCheck],
            ['Orcamento inteligente', 'Preco e prazo calculados na API com regras do produto.', ReceiptText],
            ['Pedidos', 'Pedido salvo com status, pagamento preparado e historico.', PackageCheck],
            ['Estoque', 'Materiais e movimentacoes administradas no painel.', Boxes],
          ].map(([title, text, Icon]) => (
            <article className="feature-card" key={title}><Icon size={24} /><h3>{title}</h3><p>{text}</p></article>
          ))}
        </div>
      </section>
      <section className="section tinted">
        <SectionHeading eyebrow="Catalogo" title="Produtos cadastrados no banco." />
        <ProductGrid products={products.slice(0, 4)} startQuote={startQuote} goto={goto} />
      </section>
      <ContactBand />
    </main>
  )
}

function CatalogPage({ goto, products, startQuote }) {
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
      <SectionHeading eyebrow="Catalogo" title="Escolha um produto para ver detalhes ou iniciar orcamento." />
      <div className="catalog-toolbar">
        <label className="search-box"><Search size={18} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Buscar produto" /></label>
        <div className="chip-row">
          {categories.map((item) => <button className={item === category ? 'chip active' : 'chip'} key={item} type="button" onClick={() => setCategory(item)}>{item}</button>)}
        </div>
      </div>
      <ProductGrid products={filtered} startQuote={startQuote} goto={goto} />
    </main>
  )
}

function ProductGrid({ products, startQuote, goto }) {
  if (!products.length) return <p className="muted">Nenhum produto encontrado.</p>
  return (
    <div className="product-grid">
      {products.map((product) => (
        <article className="product-card" key={product.id}>
          <img src={product.imageUrl} alt="" />
          <div><span className="tag">{product.category}</span><h3>{product.name}</h3><p>{product.description}</p></div>
          <div className="product-meta"><strong>{formatMoney(product.basePrice)}</strong><span>{pluralizeDays(product.baseDeadline)}</span></div>
          <div className="row-actions">
            <button className="ghost-button" type="button" onClick={() => goto(`/produto/${product.slug}`)}>Detalhes</button>
            <button className="primary-button" type="button" onClick={() => startQuote(product.id)}>Orcar</button>
          </div>
        </article>
      ))}
    </div>
  )
}

function ProductDetail({ productId, products, startQuote, goto }) {
  const product = products.find((item) => item.slug === productId || item.id === productId) ?? products[0]
  if (!product) return <main className="page-shell"><p>Carregando produto...</p></main>
  return (
    <main className="page-shell">
      <button className="link-button" type="button" onClick={() => goto('/catalogo')}>Catalogo</button>
      <section className="detail-layout">
        <img className="detail-image" src={product.imageUrl} alt="" />
        <div className="detail-copy">
          <span className="tag">{product.category}</span>
          <h1>{product.name}</h1>
          <p>{product.description}</p>
          <div className="detail-list">
            <span>Preco base: <strong>{formatMoney(product.basePrice)}</strong></span>
            <span>Prazo base: <strong>{pluralizeDays(product.baseDeadline)}</strong></span>
            <span>Upload de arte: <strong>{product.allowUpload ? 'Permitido' : 'Nao aplicavel'}</strong></span>
            <span>Pagamento no balcao futuro: <strong>{product.allowPickupPayment ? 'Preparado' : 'Nao permitido'}</strong></span>
          </div>
          <button className="primary-button xl" type="button" onClick={() => startQuote(product.id)}>Iniciar orcamento</button>
        </div>
      </section>
    </main>
  )
}

function QuotePage({ product, products, setProductId, goto, session }) {
  const [config, setConfig] = useState(null)
  const [quote, setQuote] = useState(null)
  const [message, setMessage] = useState('')

  useEffect(() => {
    if (product) {
      setConfig({
        productId: product.id,
        quantity: product.quantities?.[0] || 1,
        size: product.sizes?.[0]?.name || '',
        material: product.materials?.[0]?.name || '',
        printMode: product.printModes?.[0]?.name || '',
        finishing: product.finishings?.[0]?.name || '',
        urgency: 'normal',
        delivery: 'Pickup',
        paymentMethod: 'Pix',
        notes: '',
        artworkFileName: '',
      })
      setQuote(null)
    }
  }, [product?.id])

  useEffect(() => {
    if (!config?.productId) return
    api.quote({
      productId: config.productId,
      quantity: Number(config.quantity),
      size: config.size,
      material: config.material,
      printMode: config.printMode,
      finishing: config.finishing,
      urgency: config.urgency,
      delivery: config.delivery,
    }).then(setQuote).catch((error) => setMessage(error.message))
  }, [config?.productId, config?.quantity, config?.size, config?.material, config?.printMode, config?.finishing, config?.urgency, config?.delivery])

  if (!product || !config) return <main className="page-shell"><p>Carregando produtos...</p></main>

  const updateProduct = (id) => {
    setProductId(id)
  }

  const saveQuote = async () => {
    if (!session) return goto('/login')
    const saved = await api.saveQuote(config)
    setMessage(`Orcamento ${saved.number} salvo.`)
  }

  const createOrder = async () => {
    if (!session) return goto('/login')
    const order = await api.createOrder(config)
    setMessage(`Pedido #${order.number} criado.`)
    goto('/cliente/pedidos')
  }

  return (
    <main className="page-shell">
      <SectionHeading eyebrow="Orcamento automatico" title="Configure e confirme somente no final." />
      <section className="quote-layout">
        <form className="quote-form">
          <Select label="Produto" value={config.productId} onChange={updateProduct} options={products.map((item) => [item.id, item.name])} />
          <Select label="Quantidade" value={config.quantity} onChange={(quantity) => setConfig({ ...config, quantity })} options={product.quantities.map((item) => [item, item.toLocaleString('pt-BR')])} />
          <Select label="Tamanho" value={config.size} onChange={(size) => setConfig({ ...config, size })} options={toProductOption(product.sizes)} />
          <Select label="Material/papel" value={config.material} onChange={(material) => setConfig({ ...config, material })} options={toProductOption(product.materials)} />
          <Select label="Impressao" value={config.printMode} onChange={(printMode) => setConfig({ ...config, printMode })} options={toProductOption(product.printModes)} />
          <Select label="Acabamento" value={config.finishing} onChange={(finishing) => setConfig({ ...config, finishing })} options={toProductOption(product.finishings)} />
          <Select label="Urgencia" value={config.urgency} onChange={(urgency) => setConfig({ ...config, urgency })} options={[['normal', 'Normal'], ['expressa', 'Expressa +25%'], ['urgente', 'Urgente +45%']]} />
          <Select label="Retirada ou entrega" value={config.delivery} onChange={(delivery) => setConfig({ ...config, delivery })} options={[['Pickup', 'Retirada'], ['LocalDelivery', 'Entrega local']]} />
          <label>Upload da arte<input type="file" onChange={(event) => setConfig({ ...config, artworkFileName: event.target.files?.[0]?.name || '' })} /></label>
          <label>Observacoes<textarea value={config.notes} onChange={(event) => setConfig({ ...config, notes: event.target.value })} /></label>
        </form>
        <aside className="quote-summary">
          <span className="tag">Resumo</span>
          <h2>{formatMoney(quote?.total || 0)}</h2>
          <p>Prazo estimado: <strong>{pluralizeDays(quote?.estimatedDays || 0)}</strong></p>
          <dl>
            <div><dt>Subtotal</dt><dd>{formatMoney(quote?.subtotal || 0)}</dd></div>
            <div><dt>Urgencia</dt><dd>{formatMoney(quote?.urgencyFee || 0)}</dd></div>
            <div><dt>Entrega</dt><dd>{formatMoney(quote?.deliveryFee || 0)}</dd></div>
          </dl>
          <Select label="Pagamento" value={config.paymentMethod} onChange={(paymentMethod) => setConfig({ ...config, paymentMethod })} options={[['Pix', 'Pix pendente'], ['Card', 'Cartao pendente'], ...(product.allowPickupPayment ? [['Pickup', 'Pagamento no balcao']] : [])]} />
          <p className="service-note">Pix e cartao ficam pendentes ate a integracao real. Pagamento no balcao entra como pago.</p>
          {message && <p className="service-note">{message}</p>}
          <button className="ghost-button xl" type="button" onClick={saveQuote}>Salvar orcamento</button>
          <button className="primary-button xl" type="button" onClick={createOrder}>Confirmar pedido <PackageCheck size={18} /></button>
        </aside>
      </section>
    </main>
  )
}

function AuthPage({ auth, mode }) {
  const [form, setForm] = useState({ name: '', email: 'contato@studiobella.com.br', phone: '(11) 98888-4211', document: '', address: '', password: 'Cliente@123456', confirmPassword: 'Cliente@123456' })
  const [error, setError] = useState('')
  const [showPassword, setShowPassword] = useState(false)

  async function submit(event) {
    event.preventDefault()
    setError('')
    if (mode === 'signup' && form.password !== form.confirmPassword) {
      setError('As senhas nao conferem.')
      return
    }
    try {
      const payload = mode === 'signup' ? await api.register(form) : await api.login({ email: form.email, password: form.password })
      auth.setLoggedUser(payload)
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <main className="auth-shell">
      <form className="auth-card" onSubmit={submit}>
        <span className="brand-mark"><ShieldCheck size={22} /></span>
        <h1>{mode === 'signup' ? 'Criar conta de cliente' : 'Entrar na plataforma'}</h1>
        <p>{mode === 'signup' ? 'Cadastro real no banco MySQL.' : 'Login real via API e JWT.'}</p>
        <div className="auth-form">
          {mode === 'signup' && <label>Nome<input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required /></label>}
          <label>Email<input value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} required /></label>
          <PasswordField label="Senha" value={form.password} show={showPassword} setShow={setShowPassword} onChange={(password) => setForm({ ...form, password })} required />
          {mode === 'signup' && <PasswordField label="Confirmar senha" value={form.confirmPassword} show={showPassword} setShow={setShowPassword} onChange={(confirmPassword) => setForm({ ...form, confirmPassword })} required />}
          {mode === 'signup' && <label>Telefone<input value={form.phone} onChange={(event) => setForm({ ...form, phone: event.target.value })} required /></label>}
          {mode === 'signup' && <label>CPF/CNPJ<input value={form.document} onChange={(event) => setForm({ ...form, document: event.target.value })} /></label>}
          {mode === 'signup' && <label>Endereco<input value={form.address} onChange={(event) => setForm({ ...form, address: event.target.value })} /></label>}
        </div>
        {error && <p className="service-note warning">{error}</p>}
        <button className="primary-button xl" type="submit">{mode === 'signup' ? 'Cadastrar' : 'Entrar'}</button>
        {mode === 'login' && <button className="link-button" type="button" onClick={() => navigate('/cadastro', () => window.location.assign('/cadastro'))}>Nao tem uma conta? Cadastre-se</button>}
        <button className="link-button" type="button" onClick={() => navigate('/esqueci-senha', () => window.location.assign('/esqueci-senha'))}>Esqueci minha senha</button>
      </form>
    </main>
  )
}

function PasswordField({ label, value, onChange, show, setShow, required = false }) {
  return (
    <label>{label}
      <span className="password-field">
        <input type={show ? 'text' : 'password'} value={value} onChange={(event) => onChange(event.target.value)} required={required} />
        <button className="icon-button" type="button" onClick={() => setShow(!show)} title={show ? 'Ocultar senha' : 'Mostrar senha'}>
          {show ? <EyeOff size={17} /> : <Eye size={17} />}
        </button>
      </span>
    </label>
  )
}

function PasswordRecovery({ route }) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')
  const token = new URLSearchParams(window.location.search).get('token')
  const isReset = route === '/recuperar-senha' && token

  const submit = async (event) => {
    event.preventDefault()
    if (isReset) {
      await api.resetPassword({ token, password })
      setMessage('Senha redefinida. Voce ja pode fazer login.')
    } else {
      const response = await api.forgotPassword({ email })
      setMessage(response.message)
    }
  }

  return (
    <main className="auth-shell">
      <form className="auth-card" onSubmit={submit}>
        <h1>{isReset ? 'Criar nova senha' : 'Recuperar senha'}</h1>
        {isReset ? <label>Nova senha<input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required /></label> : <label>Email<input value={email} onChange={(event) => setEmail(event.target.value)} required /></label>}
        {message && <p className="service-note">{message}</p>}
        <button className="primary-button xl" type="submit">Enviar</button>
      </form>
    </main>
  )
}

function ClientArea({ goto, auth, active, products, startQuote }) {
  const [orders, setOrders] = useState([])
  const [quotes, setQuotes] = useState([])
  const [profile, setProfile] = useState(null)
  const [settings, setSettings] = useState({})
  const [paymentConfig, setPaymentConfig] = useState(null)

  const reload = () => {
    api.myOrders().then(setOrders).catch(() => setOrders([]))
    api.myQuotes().then(setQuotes).catch(() => setQuotes([]))
    api.profile().then(setProfile).catch(() => setProfile(null))
    api.publicSettings().then(setSettings).catch(() => setSettings({}))
    api.mercadoPagoConfig().then(setPaymentConfig).catch(() => setPaymentConfig(null))
  }

  useEffect(reload, [])

  const totalSpent = orders.filter((order) => order.paymentStatus === 'Paid').reduce((sum, order) => sum + order.total, 0)
  const pending = orders.filter((order) => order.paymentStatus !== 'Paid').length

  return (
    <WorkspaceShell title="Area do cliente" routes={clientRoutes} active={active} base="/cliente" goto={goto} auth={auth}>
      {active === 'pedidos' && <ClientOrders orders={orders} goto={goto} products={products} reload={reload} settings={settings} paymentConfig={paymentConfig} />}
      {active === 'orcamentos' && <ClientQuotes quotes={quotes} reload={reload} products={products} settings={settings} />}
      {active === 'perfil' && <ProfileForm profile={profile} reload={reload} />}
      {(!active || active === 'dashboard') && (
        <>
          <div className="stats-grid">
            <MetricCard label="Pedidos em andamento" value={orders.filter((order) => !['Finished', 'Cancelled'].includes(order.status)).length} tone="blue" />
            <MetricCard label="Pedidos concluidos" value={orders.filter((order) => order.status === 'Finished').length} tone="green" />
            <MetricCard label="Valor total gasto" value={formatMoney(totalSpent)} tone="green" />
            <MetricCard label="Pagamentos pendentes" value={pending} tone="amber" />
          </div>
          <div className="panel-grid two">
            <Panel title="Pedidos recentes"><ClientOrders orders={orders.slice(0, 5)} compact goto={goto} products={products} reload={reload} settings={settings} paymentConfig={paymentConfig} /></Panel>
            <Panel title="Orcamentos recentes"><ClientQuotes quotes={quotes.slice(0, 4)} compact reload={reload} products={products} settings={settings} /></Panel>
          </div>
        </>
      )}
    </WorkspaceShell>
  )
}

function configFromRecord(record) {
  return {
    productId: record.productId,
    quantity: record.quantity,
    size: record.size,
    material: record.material,
    printMode: record.printMode,
    finishing: record.finishing,
    urgency: record.urgency || 'normal',
    delivery: record.delivery || 'Pickup',
    paymentMethod: record.paymentMethod || 'Pix',
    notes: record.notes || '',
    artworkFileName: '',
  }
}

function ClientOrders({ orders, compact = false, goto, products, reload, settings, paymentConfig }) {
  const [editing, setEditing] = useState(null)
  const [form, setForm] = useState(null)
  const [payingOrder, setPayingOrder] = useState(null)
  const [pixPayment, setPixPayment] = useState(null)
  const [message, setMessage] = useState('')
  const totals = useMemo(() => ({
    total: orders.reduce((sum, order) => sum + order.total, 0),
    paid: orders.filter((order) => order.paymentStatus === 'Paid').reduce((sum, order) => sum + order.total, 0),
    pending: orders.filter((order) => order.paymentStatus === 'Pending').reduce((sum, order) => sum + order.total, 0),
  }), [orders])

  const startEdit = (order) => {
    setEditing(order)
    setForm(configFromRecord(order))
  }

  async function saveEdit(event) {
    event.preventDefault()
    await api.updateOrder(editing.id, form)
    setMessage('Pedido atualizado.')
    setEditing(null)
    reload()
  }

  async function cancelOrder(order) {
    await api.cancelOrder(order.id)
    setMessage('Pedido cancelado.')
    reload()
  }

  async function refundOrder(order) {
    await api.refundOrder(order.id)
    setMessage('Estorno solicitado.')
    reload()
  }

  async function startPix(order) {
    setPayingOrder(order)
    setPixPayment(null)
    const response = await api.createPixPayment(order.id)
    setPixPayment(response)
    reload()
  }

  function startCard(order) {
    setPayingOrder(order)
    setPixPayment(null)
    if (paymentConfig?.publicKey) {
      initMercadoPago(paymentConfig.publicKey, { locale: 'pt-BR' })
    }
  }

  async function submitCard(paymentData) {
    const payer = paymentData?.payer || {}
    const response = await api.payWithCard(payingOrder.id, {
      token: paymentData.token,
      paymentMethodId: paymentData.payment_method_id,
      issuerId: paymentData.issuer_id ? Number(paymentData.issuer_id) : null,
      installments: Number(paymentData.installments || 1),
      payerEmail: payer.email,
      identificationType: payer.identification?.type || null,
      identificationNumber: payer.identification?.number || null,
    })
    setMessage(response.paymentStatus === 'Paid' ? 'Pagamento aprovado.' : `Pagamento ${translateStatus(response.paymentStatus)}.`)
    reload()
  }

  return (
    <Panel title="Meus pedidos" action={<button className="primary-button" type="button" onClick={() => goto('/orcamento')}><PackagePlus size={16} /> Novo pedido</button>}>
      {!compact && (
        <div className="stats-grid compact">
          <MetricCard label="Total dos pedidos" value={formatMoney(totals.total)} tone="blue" />
          <MetricCard label="Pagos" value={formatMoney(totals.paid)} tone="green" />
          <MetricCard label="Pendentes" value={formatMoney(totals.pending)} tone="amber" />
        </div>
      )}
      {message && <p className="service-note">{message}</p>}
      {editing && form && (
        <InlineConfigurator title={`Editar pedido #${editing.number}`} form={form} setForm={setForm} products={products} onSubmit={saveEdit} onCancel={() => setEditing(null)} submitLabel="Salvar pedido" includePayment />
      )}
      {!compact && payingOrder && payingOrder.paymentMethod === 'Pix' && pixPayment && (
        <PaymentBox title={`Pix do pedido #${payingOrder.number}`} onClose={() => setPayingOrder(null)}>
          {pixPayment.qrCodeBase64 && <img className="pix-qr" src={`data:image/png;base64,${pixPayment.qrCodeBase64}`} alt="QR Code Pix" />}
          {pixPayment.qrCode && <label>Copia e cola Pix<textarea readOnly value={pixPayment.qrCode} /></label>}
          {pixPayment.ticketUrl && <a className="ghost-button" href={pixPayment.ticketUrl} target="_blank" rel="noreferrer">Abrir pagamento</a>}
        </PaymentBox>
      )}
      {!compact && payingOrder && payingOrder.paymentMethod === 'Card' && (
        <PaymentBox title={`Cartao do pedido #${payingOrder.number}`} onClose={() => setPayingOrder(null)}>
          {paymentConfig?.publicKey ? (
            <CardPayment
              initialization={{ amount: payingOrder.total }}
              locale="pt-BR"
              onSubmit={submitCard}
              onError={(error) => setMessage(error?.message || 'Falha no formulario do Mercado Pago.')}
            />
          ) : (
            <p className="service-note warning">Configure a Public Key do Mercado Pago para habilitar cartao.</p>
          )}
        </PaymentBox>
      )}
      <DataTable columns={compact ? ['Pedido', 'Produto', 'Status'] : ['Pedido', 'Data', 'Produto', 'Qtd', 'Prazo', 'Pagamento', 'Status', 'Valor', 'Acao']} rows={orders.map((order) => compact
        ? [`#${order.number}`, order.productName, order.status]
        : [`#${order.number}`, formatDate(order.createdAt), order.productName, order.quantity, order.deadline ? formatDate(order.deadline) : '-', order.paymentStatus, order.status, formatMoney(order.total),
          <span className="row-actions">
            {order.paymentStatus === 'Pending' && order.paymentMethod === 'Pix' && <button className="primary-button" type="button" onClick={() => startPix(order)}>Pagar Pix</button>}
            {order.paymentStatus === 'Pending' && order.paymentMethod === 'Card' && <button className="primary-button" type="button" onClick={() => startCard(order)}>Pagar cartao</button>}
            {settings.allowCustomerOrderEdit && !['InProduction', 'Finished', 'Cancelled'].includes(order.status) && <button className="ghost-button" type="button" onClick={() => startEdit(order)}>Editar</button>}
            {settings.allowCustomerOrderCancellation && !['InProduction', 'Finished', 'Cancelled'].includes(order.status) && <button className="ghost-button danger-action" type="button" onClick={() => cancelOrder(order)}>Cancelar</button>}
            {settings.allowCustomerRefundRequest && order.paymentStatus === 'Paid' && <button className="ghost-button danger-action" type="button" onClick={() => refundOrder(order)}>Estorno</button>}
          </span>])} />
    </Panel>
  )
}

function PaymentBox({ title, onClose, children }) {
  return (
    <section className="payment-box">
      <div className="panel-heading"><h3>{title}</h3><button className="ghost-button" type="button" onClick={onClose}>Fechar</button></div>
      {children}
    </section>
  )
}

function ClientQuotes({ quotes, compact = false, reload, products, settings }) {
  const [editing, setEditing] = useState(null)
  const [form, setForm] = useState(null)
  async function convert(quoteId) {
    await api.convertQuote(quoteId, { paymentMethod: 'Pix', artworkFileName: null })
    reload()
  }
  const startEdit = (quote) => {
    setEditing(quote)
    setForm(configFromRecord(quote))
  }
  async function saveEdit(event) {
    event.preventDefault()
    await api.updateQuote(editing.id, form)
    setEditing(null)
    reload()
  }
  return (
    <Panel title="Meus orcamentos">
      {editing && form && <InlineConfigurator title={`Editar ${editing.number}`} form={form} setForm={setForm} products={products} onSubmit={saveEdit} onCancel={() => setEditing(null)} submitLabel="Salvar orcamento" />}
      <DataTable columns={compact ? ['Orcamento', 'Produto', 'Valor'] : ['Orcamento', 'Data', 'Produto', 'Qtd', 'Prazo', 'Status', 'Valor', 'Acao']} rows={quotes.map((quote) => compact
        ? [quote.number, quote.productName, formatMoney(quote.total)]
        : [quote.number, formatDate(quote.createdAt), quote.productName, quote.quantity, pluralizeDays(quote.estimatedDays), quote.status, formatMoney(quote.total), quote.status === 'ConvertedToOrder' ? 'Convertido' : <span className="row-actions">{settings.allowCustomerQuoteEdit && <button className="ghost-button" type="button" onClick={() => startEdit(quote)}>Editar</button>}<button className="ghost-button" type="button" onClick={() => convert(quote.id)}>Converter</button></span>])} />
    </Panel>
  )
}

function InlineConfigurator({ title, form, setForm, products, onSubmit, onCancel, submitLabel, includePayment = false }) {
  const product = products.find((item) => item.id === form.productId) || products[0]
  if (!product) return null
  const paymentOptions = [['Pix', 'Pix pendente'], ['Card', 'Cartao pendente'], ...(product.allowPickupPayment ? [['Pickup', 'Pagamento no balcao']] : [])]
  const changeProduct = (productId) => {
    const next = products.find((item) => item.id === productId)
    setForm({
      ...form,
      productId,
      quantity: next?.quantities?.[0] || 1,
      size: next?.sizes?.[0]?.name || '',
      material: next?.materials?.[0]?.name || '',
      printMode: next?.printModes?.[0]?.name || '',
      finishing: next?.finishings?.[0]?.name || '',
      paymentMethod: next?.allowPickupPayment ? form.paymentMethod : form.paymentMethod === 'Pickup' ? 'Pix' : form.paymentMethod,
    })
  }
  return (
    <form className="inline-editor" onSubmit={onSubmit}>
      <h3>{title}</h3>
      <Select label="Produto" value={form.productId} onChange={changeProduct} options={products.map((item) => [item.id, item.name])} />
      <Select label="Quantidade" value={form.quantity} onChange={(quantity) => setForm({ ...form, quantity: Number(quantity) })} options={(product.quantities || []).map((item) => [item, item.toLocaleString('pt-BR')])} />
      <Select label="Tamanho" value={form.size} onChange={(size) => setForm({ ...form, size })} options={toProductOption(product.sizes)} />
      <Select label="Material" value={form.material} onChange={(material) => setForm({ ...form, material })} options={toProductOption(product.materials)} />
      <Select label="Impressao" value={form.printMode} onChange={(printMode) => setForm({ ...form, printMode })} options={toProductOption(product.printModes)} />
      <Select label="Acabamento" value={form.finishing} onChange={(finishing) => setForm({ ...form, finishing })} options={toProductOption(product.finishings)} />
      <Select label="Urgencia" value={form.urgency} onChange={(urgency) => setForm({ ...form, urgency })} options={[['normal', 'Normal'], ['expressa', 'Expressa +25%'], ['urgente', 'Urgente +45%']]} />
      <Select label="Retirada ou entrega" value={form.delivery} onChange={(delivery) => setForm({ ...form, delivery })} options={[['Pickup', 'Retirada'], ['LocalDelivery', 'Entrega local']]} />
      {includePayment && <Select label="Pagamento" value={form.paymentMethod} onChange={(paymentMethod) => setForm({ ...form, paymentMethod })} options={paymentOptions} />}
      <label className="wide-field">Observacoes<textarea value={form.notes || ''} onChange={(event) => setForm({ ...form, notes: event.target.value })} /></label>
      <div className="row-actions"><button className="primary-button" type="submit">{submitLabel}</button><button className="ghost-button" type="button" onClick={onCancel}>Fechar</button></div>
    </form>
  )
}

function ProfileForm({ profile, reload }) {
  const [form, setForm] = useState(profile || {})
  const [message, setMessage] = useState('')
  useEffect(() => setForm(profile || {}), [profile])
  if (!profile) return <Panel title="Perfil"><p>Carregando perfil...</p></Panel>
  async function submit(event) {
    event.preventDefault()
    await api.updateProfile(form)
    setMessage('Perfil atualizado.')
    reload()
  }
  return (
    <Panel title="Meu perfil">
      <form className="settings-grid" onSubmit={submit}>
        <label>Nome<input value={form.name || ''} onChange={(event) => setForm({ ...form, name: event.target.value })} /></label>
        <label>Telefone<input value={form.phone || ''} onChange={(event) => setForm({ ...form, phone: event.target.value })} /></label>
        <label>CPF/CNPJ<input value={form.document || ''} onChange={(event) => setForm({ ...form, document: event.target.value })} /></label>
        <label>Endereco<input value={form.address || ''} onChange={(event) => setForm({ ...form, address: event.target.value })} /></label>
        <label>Preferencia de contato<input value={form.contactPreference || ''} onChange={(event) => setForm({ ...form, contactPreference: event.target.value })} /></label>
        <label>Nova senha<input type="password" value={form.password || ''} onChange={(event) => setForm({ ...form, password: event.target.value })} /></label>
        {message && <p className="service-note">{message}</p>}
        <button className="primary-button" type="submit">Salvar perfil</button>
      </form>
    </Panel>
  )
}

function AdminArea({ goto, auth, active, products, refreshProducts }) {
  const [dashboard, setDashboard] = useState(null)
  const [orders, setOrders] = useState([])
  const [customers, setCustomers] = useState([])
  const [inventory, setInventory] = useState([])
  const [adminProducts, setAdminProducts] = useState(products)
  const [settings, setSettings] = useState(null)

  const reload = () => {
    api.adminDashboard().then(setDashboard).catch(() => {})
    api.adminOrders().then(setOrders).catch(() => setOrders([]))
    api.adminCustomers().then(setCustomers).catch(() => setCustomers([]))
    api.adminInventory().then(setInventory).catch(() => setInventory([]))
    api.adminProducts().then(setAdminProducts).catch(() => setAdminProducts(products))
    api.adminSettings().then(setSettings).catch(() => {})
  }
  useEffect(reload, [])
  useEffect(() => setAdminProducts(products), [products])

  const reloadProducts = () => {
    api.adminProducts().then(setAdminProducts).catch(() => {})
    refreshProducts()
  }

  return (
    <WorkspaceShell title="Admin CRM" routes={adminRoutes} active={active} base="/admin" goto={goto} auth={auth}>
      {active === 'pedidos' && <OrdersAdmin orders={orders} reload={reload} />}
      {active === 'clientes' && <ClientsAdmin customers={customers} />}
      {active === 'produtos' && <ProductsAdmin products={adminProducts} reload={reloadProducts} />}
      {active === 'estoque' && <InventoryAdmin inventory={inventory} reload={reload} />}
      {active === 'producao' && <ProductionAdmin orders={orders} />}
      {active === 'pagamentos' && <PaymentsAdmin orders={orders} reload={reload} />}
      {active === 'entregas' && <DeliveriesAdmin orders={orders} />}
      {active === 'relatorios' && <ReportsAdmin dashboard={dashboard} inventory={inventory} />}
      {active === 'configuracoes' && <SettingsAdmin settings={settings} reload={reload} />}
      {(!active || active === 'dashboard') && <DashboardAdmin dashboard={dashboard} orders={orders} customers={customers} inventory={inventory} />}
    </WorkspaceShell>
  )
}

function WorkspaceShell({ title, routes, active, base, goto, auth, children }) {
  return (
    <main className="workspace-shell">
      <aside className="sidebar">
        <button className="workspace-brand" type="button" onClick={() => goto('/')}>
          <span className="brand-mark"><Printer size={20} /></span>
          <span>{company.name}<small>Sistema real</small></span>
        </button>
        <nav className="side-nav">
          {routes.map(([id, label, Icon]) => <button key={id} className={active === id ? 'active' : ''} type="button" onClick={() => goto(`${base}/${id}`)}><Icon size={18} /> {label}</button>)}
        </nav>
      </aside>
      <section className="workspace-main">
        <header className="workspace-topbar">
          <div><p className="eyebrow">{title}</p><h1>{routes.find(([id]) => id === active)?.[1] || 'Dashboard'}</h1></div>
          <button className="ghost-button" type="button" onClick={auth.logout}>Sair</button>
        </header>
        {children}
      </section>
    </main>
  )
}

function DashboardAdmin({ dashboard, orders, customers, inventory }) {
  return (
    <>
      <div className="stats-grid">
        <MetricCard label="Pedidos de hoje" value={dashboard?.ordersToday ?? 0} tone="blue" />
        <MetricCard label="Em producao" value={dashboard?.inProduction ?? 0} tone="green" />
        <MetricCard label="Orcamentos pendentes" value={dashboard?.waitingPayment ?? 0} tone="amber" />
        <MetricCard label="Faturamento do mes" value={formatMoney(dashboard?.revenueMonth ?? 0)} tone="green" />
        <MetricCard label="Clientes" value={customers.length} tone="blue" />
        <MetricCard label="Estoque baixo" value={inventory.filter((item) => item.available < item.minimum).length} tone="red" />
      </div>
      <div className="panel-grid two">
        <Panel title="Pedidos recentes"><DataTable columns={['Pedido', 'Cliente', 'Status']} rows={orders.slice(0, 6).map((order) => [`#${order.number}`, order.customerName, order.status])} /></Panel>
        <Panel title="Alertas de estoque">{inventory.filter((item) => item.available < item.minimum).map((item) => <p className="alert-line" key={item.id}><AlertTriangle size={16} /> {item.name} abaixo do minimo</p>)}</Panel>
      </div>
    </>
  )
}

function OrdersAdmin({ orders, reload }) {
  async function updateStatus(id, status) {
    await api.updateOrderStatus(id, { status, internalNotes: '', adminPassword: '' })
    reload()
  }
  return (
    <Panel title="Gestao de pedidos">
      <DataTable columns={['Pedido', 'Cliente', 'Produto', 'Prazo', 'Pagamento', 'Status', 'Acao']} rows={orders.map((order) => [
        `#${order.number}`, order.customerName, order.productName, order.deadline ? formatDate(order.deadline) : '-', order.paymentStatus, order.status,
        <button className="ghost-button" type="button" onClick={() => updateStatus(order.id, 'InProduction')}>Em producao</button>,
      ])} />
    </Panel>
  )
}

function ClientsAdmin({ customers }) {
  return <Panel title="Clientes"><DataTable columns={['Nome', 'Email', 'Telefone', 'Total gasto', 'Status']} rows={customers.map((client) => [client.name, client.email, client.phone, formatMoney(client.totalSpent || 0), client.active ? 'Ativo' : 'Inativo'])} /></Panel>
}

const emptyProductForm = {
  slug: '',
  name: '',
  category: '',
  description: '',
  imageUrl: '',
  basePrice: 0,
  baseDeadline: 3,
  allowUpload: true,
  allowPickup: true,
  allowDelivery: true,
  allowPickupPayment: false,
  requiresAdvancePayment: true,
  active: true,
  quantitiesText: '100, 250, 500',
  sizesText: 'Padrao|0|0',
  materialsText: 'Couchê 250g|0|0',
  printModesText: '4x0|0|0',
  finishingsText: 'Sem acabamento|0|0',
}

function productToForm(product) {
  const pack = (items) => items.map((item) => `${item.name}|${item.price}|${item.days}`).join('\n')
  return {
    ...emptyProductForm,
    ...product,
    baseDeadline: product.baseDeadline,
    quantitiesText: product.quantities?.join(', ') || '',
    sizesText: pack(product.sizes || []),
    materialsText: pack(product.materials || []),
    printModesText: pack(product.printModes || []),
    finishingsText: pack(product.finishings || []),
  }
}

function formToProduct(form) {
  const parseOptions = (text) => String(text || '').split('\n').map((line) => {
    const [name, price = '0', days = '0'] = line.split('|')
    return { name: name?.trim() || '', price: Number(price), days: Number(days) }
  }).filter((item) => item.name)

  return {
    slug: form.slug,
    name: form.name,
    category: form.category,
    description: form.description,
    imageUrl: form.imageUrl,
    basePrice: Number(form.basePrice),
    baseDeadline: Number(form.baseDeadline),
    allowUpload: Boolean(form.allowUpload),
    allowPickup: Boolean(form.allowPickup),
    allowDelivery: Boolean(form.allowDelivery),
    allowPickupPayment: Boolean(form.allowPickupPayment),
    requiresAdvancePayment: Boolean(form.requiresAdvancePayment),
    active: Boolean(form.active),
    quantities: String(form.quantitiesText || '').split(',').map((item) => Number(item.trim())).filter(Boolean),
    sizes: parseOptions(form.sizesText),
    materials: parseOptions(form.materialsText),
    printModes: parseOptions(form.printModesText),
    finishings: parseOptions(form.finishingsText),
  }
}

function ProductsAdmin({ products, reload }) {
  const [form, setForm] = useState(emptyProductForm)
  const [editingId, setEditingId] = useState(null)
  const [message, setMessage] = useState('')

  async function submit(event) {
    event.preventDefault()
    const payload = formToProduct(form)
    if (editingId) {
      await api.updateProduct(editingId, payload)
      setMessage('Produto atualizado.')
    } else {
      await api.createProduct(payload)
      setMessage('Produto criado.')
    }
    setForm(emptyProductForm)
    setEditingId(null)
    reload()
  }

  async function remove(product) {
    await api.deleteProduct(product.id)
    setMessage('Produto removido ou inativado.')
    reload()
  }

  return (
    <Panel title="Catalogo e produtos">
      <form className="product-admin-form" onSubmit={submit}>
        <label>Nome<input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required /></label>
        <label>Slug<input value={form.slug} onChange={(event) => setForm({ ...form, slug: event.target.value })} required /></label>
        <label>Categoria<input value={form.category} onChange={(event) => setForm({ ...form, category: event.target.value })} required /></label>
        <label>Imagem URL<input value={form.imageUrl} onChange={(event) => setForm({ ...form, imageUrl: event.target.value })} required /></label>
        <label>Preco base<input type="number" step="0.01" value={form.basePrice} onChange={(event) => setForm({ ...form, basePrice: event.target.value })} /></label>
        <label>Prazo base<input type="number" value={form.baseDeadline} onChange={(event) => setForm({ ...form, baseDeadline: event.target.value })} /></label>
        <label className="wide-field">Descricao<textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} required /></label>
        <label>Quantidades<input value={form.quantitiesText} onChange={(event) => setForm({ ...form, quantitiesText: event.target.value })} /></label>
        <label>Tamanhos<textarea value={form.sizesText} onChange={(event) => setForm({ ...form, sizesText: event.target.value })} /></label>
        <label>Materiais<textarea value={form.materialsText} onChange={(event) => setForm({ ...form, materialsText: event.target.value })} /></label>
        <label>Impressoes<textarea value={form.printModesText} onChange={(event) => setForm({ ...form, printModesText: event.target.value })} /></label>
        <label>Acabamentos<textarea value={form.finishingsText} onChange={(event) => setForm({ ...form, finishingsText: event.target.value })} /></label>
        <label><input type="checkbox" checked={form.active} onChange={(event) => setForm({ ...form, active: event.target.checked })} /> Ativo</label>
        <label><input type="checkbox" checked={form.allowPickupPayment} onChange={(event) => setForm({ ...form, allowPickupPayment: event.target.checked, requiresAdvancePayment: !event.target.checked })} /> Permitir pagamento no balcao</label>
        <div className="row-actions">
          <button className="primary-button" type="submit">{editingId ? 'Salvar produto' : 'Adicionar produto'}</button>
          {editingId && <button className="ghost-button" type="button" onClick={() => { setEditingId(null); setForm(emptyProductForm) }}>Cancelar edicao</button>}
        </div>
      </form>
      {message && <p className="service-note">{message}</p>}
      <DataTable columns={['Produto', 'Categoria', 'Preco', 'Prazo', 'Status', 'Acao']} rows={products.map((product) => [
        product.name,
        product.category,
        formatMoney(product.basePrice),
        pluralizeDays(product.baseDeadline),
        product.active ? 'Ativo' : 'Inativo',
        <span className="row-actions"><button className="ghost-button" type="button" onClick={() => { setEditingId(product.id); setForm(productToForm(product)) }}>Editar</button><button className="ghost-button danger-action" type="button" onClick={() => remove(product)}>Excluir</button></span>,
      ])} />
    </Panel>
  )
}

function InventoryAdmin({ inventory, reload }) {
  const [movement, setMovement] = useState({ inventoryItemId: '', type: 'in', quantity: 1, reason: 'Ajuste manual' })
  async function submit(event) {
    event.preventDefault()
    await api.stockMovement(movement)
    reload()
  }
  return (
    <Panel title="Estoque e armazem">
      <form className="settings-grid" onSubmit={submit}>
        <Select label="Material" value={movement.inventoryItemId} onChange={(inventoryItemId) => setMovement({ ...movement, inventoryItemId })} options={[['', 'Selecione'], ...inventory.map((item) => [item.id, item.name])]} />
        <Select label="Tipo" value={movement.type} onChange={(type) => setMovement({ ...movement, type })} options={[['in', 'Entrada'], ['out', 'Saida'], ['waste', 'Perda/descarte'], ['adjustment', 'Ajuste']]} />
        <label>Quantidade<input type="number" value={movement.quantity} onChange={(event) => setMovement({ ...movement, quantity: Number(event.target.value) })} /></label>
        <label>Motivo<input value={movement.reason} onChange={(event) => setMovement({ ...movement, reason: event.target.value })} /></label>
        <button className="primary-button" type="submit">Registrar movimentacao</button>
      </form>
      <DataTable columns={['Material', 'Categoria', 'Disponivel', 'Minimo', 'Fornecedor', 'Custo', 'Status']} rows={inventory.map((item) => [item.name, item.category, `${item.available} ${item.unit}`, `${item.minimum} ${item.unit}`, item.supplier, formatMoney(item.unitCost), item.available < item.minimum ? 'Estoque baixo' : 'OK'])} />
    </Panel>
  )
}

function ProductionAdmin({ orders }) {
  const columns = [['WaitingArtwork', 'Aguardando arte'], ['ArtworkApproved', 'Arte aprovada'], ['InProduction', 'Em impressao'], ['Finishing', 'Em acabamento'], ['ReadyForPickup', 'Pronto'], ['Finished', 'Finalizado']]
  return (
    <div className="kanban">
      {columns.map(([status, title]) => (
        <section className="kanban-column" key={status}>
          <h3>{title}</h3>
          {orders.filter((order) => order.status === status).map((order) => (
            <article className="job-card" key={order.id}><strong>#{order.number} {order.productName}</strong><span>{order.customerName}</span><small>{order.paymentStatus}</small></article>
          ))}
        </section>
      ))}
    </div>
  )
}

function PaymentsAdmin({ orders, reload }) {
  async function confirm(order) {
    await api.confirmManualPayment(order.id, { transactionId: 'manual', receiptUrl: '', adminPassword: '' })
    reload()
  }
  return (
    <Panel title="Pagamentos preparados">
      <p className="service-note">Pix e cartao ficam pendentes ate a integracao real. Confirmacao manual aparece apenas para pagamento no balcao.</p>
      <DataTable columns={['Pedido', 'Metodo', 'Status', 'Valor', 'Acao']} rows={orders.map((order) => [`#${order.number}`, order.paymentMethod, order.paymentStatus, formatMoney(order.total), order.paymentMethod === 'Pickup' && order.paymentStatus !== 'Paid' ? <button className="ghost-button" type="button" onClick={() => confirm(order)}>Confirmar manualmente</button> : order.paymentStatus === 'Paid' ? 'Confirmado' : 'Aguardando gateway'])} />
    </Panel>
  )
}

function DeliveriesAdmin({ orders }) {
  return <Panel title="Entregas e retiradas"><DataTable columns={['Pedido', 'Cliente', 'Prazo', 'Status']} rows={orders.map((order) => [`#${order.number}`, order.customerName, order.deadline ? formatDate(order.deadline) : '-', order.status])} /></Panel>
}

function ReportsAdmin({ dashboard, inventory }) {
  return (
    <div className="panel-grid three">
      <Panel title="Faturamento do mes"><p className="big-number">{formatMoney(dashboard?.revenueMonth || 0)}</p></Panel>
      <Panel title="Pedidos em producao"><p className="big-number">{dashboard?.inProduction || 0}</p></Panel>
      <Panel title="Estoque baixo"><p className="big-number warning-text">{inventory.filter((item) => item.available < item.minimum).length}</p></Panel>
    </div>
  )
}

function SettingsAdmin({ settings, reload }) {
  const [form, setForm] = useState(settings || {})
  useEffect(() => setForm(settings || {}), [settings])
  if (!settings) return <Panel title="Configuracoes"><p>Carregando...</p></Panel>
  async function submit(event) {
    event.preventDefault()
    await api.updateAdminSettings({
      companyName: form.companyName,
      companyEmail: form.companyEmail,
      companyPhone: form.companyPhone,
      requireAdminPasswordForSensitiveActions: Boolean(form.requireAdminPasswordForSensitiveActions),
      adminActionPassword: form.adminActionPassword || null,
      autoStockDeductionEnabled: Boolean(form.autoStockDeductionEnabled),
      stockDeductionTriggerStatus: form.stockDeductionTriggerStatus || 'InProduction',
      allowCustomerQuoteEdit: Boolean(form.allowCustomerQuoteEdit),
      allowCustomerOrderEdit: Boolean(form.allowCustomerOrderEdit),
      allowCustomerOrderCancellation: Boolean(form.allowCustomerOrderCancellation),
      allowCustomerRefundRequest: Boolean(form.allowCustomerRefundRequest),
      currentAdminPassword: form.currentAdminPassword || null,
    })
    reload()
  }
  return (
    <Panel title="Configuracoes administrativas">
      <form className="settings-grid" onSubmit={submit}>
        <label>Nome da empresa<input value={form.companyName || ''} onChange={(event) => setForm({ ...form, companyName: event.target.value })} /></label>
        <label>Email<input value={form.companyEmail || ''} onChange={(event) => setForm({ ...form, companyEmail: event.target.value })} /></label>
        <label>Telefone<input value={form.companyPhone || ''} onChange={(event) => setForm({ ...form, companyPhone: event.target.value })} /></label>
        <label><input type="checkbox" checked={Boolean(form.requireAdminPasswordForSensitiveActions)} onChange={(event) => setForm({ ...form, requireAdminPasswordForSensitiveActions: event.target.checked })} /> Exigir senha extra</label>
        <label>Nova senha extra<input type="password" value={form.adminActionPassword || ''} onChange={(event) => setForm({ ...form, adminActionPassword: event.target.value })} /></label>
        <label><input type="checkbox" checked={Boolean(form.autoStockDeductionEnabled)} onChange={(event) => setForm({ ...form, autoStockDeductionEnabled: event.target.checked })} /> Baixa automatica de estoque</label>
        <Select label="Status para baixa" value={form.stockDeductionTriggerStatus || 'InProduction'} onChange={(stockDeductionTriggerStatus) => setForm({ ...form, stockDeductionTriggerStatus })} options={[['InProduction', 'Em producao'], ['Finished', 'Finalizado'], ['PaymentConfirmed', 'Pagamento confirmado']]} />
        <label><input type="checkbox" checked={Boolean(form.allowCustomerQuoteEdit)} onChange={(event) => setForm({ ...form, allowCustomerQuoteEdit: event.target.checked })} /> Cliente edita orcamentos</label>
        <label><input type="checkbox" checked={Boolean(form.allowCustomerOrderEdit)} onChange={(event) => setForm({ ...form, allowCustomerOrderEdit: event.target.checked })} /> Cliente edita pedidos</label>
        <label><input type="checkbox" checked={Boolean(form.allowCustomerOrderCancellation)} onChange={(event) => setForm({ ...form, allowCustomerOrderCancellation: event.target.checked })} /> Cliente cancela pedidos</label>
        <label><input type="checkbox" checked={Boolean(form.allowCustomerRefundRequest)} onChange={(event) => setForm({ ...form, allowCustomerRefundRequest: event.target.checked })} /> Cliente solicita estorno</label>
        <label>Senha extra atual<input type="password" value={form.currentAdminPassword || ''} onChange={(event) => setForm({ ...form, currentAdminPassword: event.target.value })} /></label>
        <button className="primary-button" type="submit">Salvar configuracoes</button>
      </form>
    </Panel>
  )
}

function DataTable({ columns, rows }) {
  return (
    <div className="table-wrap">
      <table>
        <thead><tr>{columns.map((column) => <th key={column}>{column}</th>)}</tr></thead>
        <tbody>{rows.length ? rows.map((row, index) => <tr key={`${index}-${row[0]}`}>{row.map((cell, cellIndex) => <td key={cellIndex}>{renderCell(cell)}</td>)}</tr>) : <tr><td colSpan={columns.length}>Nenhum registro.</td></tr>}</tbody>
      </table>
    </div>
  )
}

function renderCell(cell) {
  if (typeof cell !== 'string') return cell
  if (['Pending', 'CounterPayment', 'Paid', 'Finished', 'InProduction', 'WaitingPayment', 'Estoque baixo', 'OK', 'Ativo'].includes(cell)) {
    return <span className={`status-pill ${getStatusTone(cell)}`}>{translateStatus(cell)}</span>
  }
  return translateStatus(cell)
}

function translateStatus(text) {
  const map = {
    Pending: 'Pendente',
    CounterPayment: 'No balcao',
    Paid: 'Pago',
    WaitingPayment: 'Aguardando pagamento',
    PaymentConfirmed: 'Pagamento confirmado',
    WaitingArtwork: 'Aguardando arte',
    ArtworkReview: 'Arte em analise',
    ArtworkRejected: 'Arte recusada',
    ArtworkApproved: 'Arte aprovada',
    InProduction: 'Em producao',
    Finishing: 'Em acabamento',
    ReadyForPickup: 'Pronto para retirada',
    Finished: 'Finalizado',
    Cancelled: 'Cancelado',
    Draft: 'Rascunho',
    Saved: 'Salvo',
    ConvertedToOrder: 'Convertido',
  }
  return map[text] || text
}

function getStatusTone(text) {
  if (['Paid', 'Finished', 'OK', 'Ativo', 'ConvertedToOrder'].includes(text)) return 'success'
  if (['Pending', 'CounterPayment', 'WaitingPayment', 'WaitingArtwork', 'Saved'].includes(text)) return 'warning'
  if (['Cancelled', 'ArtworkRejected', 'Estoque baixo'].includes(text)) return 'danger'
  return 'info'
}

function Panel({ title, action, children }) {
  return <section className="panel"><div className="panel-heading"><h2>{title}</h2>{action}</div>{children}</section>
}

function MetricCard({ label, value, tone }) {
  return <article className={`metric-card ${tone}`}><span>{label}</span><strong>{value}</strong></article>
}

function Select({ label, value, onChange, options }) {
  return <label>{label}<select value={value ?? ''} onChange={(event) => onChange(event.target.value)}>{options.map(([optionValue, optionLabel]) => <option key={optionValue} value={optionValue}>{optionLabel}</option>)}</select></label>
}

function SectionHeading({ eyebrow, title, action }) {
  return <div className="section-heading"><div><p className="eyebrow">{eyebrow}</p><h2>{title}</h2></div>{action}</div>
}

function ContactPage() {
  return <main className="page-shell"><SectionHeading eyebrow="Contato" title="Atendimento no balcao, WhatsApp e producao local." /><ContactBand /></main>
}

function ContactBand() {
  return <section className="contact-band"><div><h2>Pronto para produzir?</h2><p>{company.address} - {company.hours}</p></div><a className="primary-button xl" href={`https://wa.me/${company.whatsapp}`} target="_blank" rel="noreferrer"><Phone size={18} /> {company.phone}</a></section>
}

export default App
